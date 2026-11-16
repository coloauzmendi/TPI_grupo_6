using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBranchStockRepository _stockRepository;
        private readonly IAccessService _accessService;

        public ProductService(
            IProductRepository productRepository,
            IBranchStockRepository stockRepository,
            IAccessService accessService)
        {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _accessService = accessService;
        }

        // Dueño: productos de sus empresas. Empleado: los de la empresa de su sucursal.
        public List<ProductResponse> GetAll(string cuit, Guid? companyId = null)
        {
            List<Guid> companyIds = GetAccessibleCompanyIds(cuit);
            if (companyId is not null)
            {
                if (!companyIds.Contains(companyId.Value))
                {
                    throw new NotAllowedException("No tenés acceso a esa empresa.");
                }

                companyIds = new List<Guid> { companyId.Value };
            }

            return _productRepository.GetByCompanies(companyIds).Select(ResponseMapper.ToResponse).ToList();
        }

        // Un producto de una empresa ajena se informa como inexistente
        public ProductResponse GetById(string cuit, Guid id)
        {
            Product product = GetActive(id);
            if (!GetAccessibleCompanyIds(cuit).Contains(product.CompanyId))
            {
                throw new NotFoundException("El producto no existe.");
            }

            return ResponseMapper.ToResponse(product);
        }

        // Si había uno dado de baja con el mismo nombre y marca en la empresa, se reactiva con los datos nuevos
        public ProductResponse Create(string cuit, ProductCreateRequest request)
        {
            Guid companyId = _accessService.EnsureCompanyOwner(cuit, request.CompanyId!.Value).Id;

            List<Product> matches = _productRepository.GetByCompanyIncludingInactive(companyId)
                .Where(p => SameText(p.Name, request.Name) && SameText(p.Brand, request.Brand))
                .ToList();

            if (matches.Any(p => p.IsActive))
            {
                throw new ConflictException("Ya existe un producto con ese nombre y marca.");
            }

            Product? inactive = matches.FirstOrDefault();
            if (inactive is null)
            {
                Product created = _productRepository.Create(new Product
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Brand = request.Brand,
                    Category = request.Category,
                    Price = request.Price,
                    Industry = request.Industry,
                    CompanyId = companyId
                });

                return ResponseMapper.ToResponse(created);
            }

            inactive.Category = request.Category;
            inactive.Price = request.Price;
            inactive.Industry = request.Industry;
            inactive.IsActive = true;
            _productRepository.Update(inactive);

            return ResponseMapper.ToResponse(inactive);
        }

        // La empresa del producto no se modifica
        public void Update(string cuit, Guid id, ProductRequest request)
        {
            Product product = GetOwned(cuit, id);

            product.Name = request.Name;
            product.Brand = request.Brand;
            product.Category = request.Category;
            product.Price = request.Price;
            product.Industry = request.Industry;
            _productRepository.Update(product);
        }

        // Baja lógica: también se da de baja su stock en todas las sucursales
        public void Delete(string cuit, Guid id)
        {
            Product product = GetOwned(cuit, id);

            _productRepository.Delete(product, _stockRepository.GetByProduct(id));
        }

        private Product GetActive(Guid id)
        {
            Product? product = _productRepository.GetById(id);
            if (product is null)
            {
                throw new NotFoundException("El producto no existe.");
            }

            return product;
        }

        private Product GetOwned(string cuit, Guid id)
        {
            Product product = GetActive(id);
            _accessService.EnsureCompanyOwner(cuit, product.CompanyId);

            return product;
        }

        private List<Guid> GetAccessibleCompanyIds(string cuit)
        {
            return _accessService.GetCompanies(cuit).Select(c => c.Id).ToList();
        }

        private static bool SameText(string a, string b)
        {
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
