using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBranchStockRepository _stockRepository;

        public ProductService(IProductRepository productRepository, IBranchStockRepository stockRepository)
        {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
        }

        public List<ProductResponse> GetAll()
        {
            return _productRepository.GetAll().Select(ResponseMapper.ToResponse).ToList();
        }

        public ProductResponse GetById(Guid id)
        {
            return ResponseMapper.ToResponse(GetActive(id));
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

        // Si había uno dado de baja con el mismo nombre y marca, se reactiva con los datos nuevos
        public ProductResponse Create(ProductRequest request)
        {
            List<Product> matches = _productRepository.GetAllIncludingInactive()
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
                    Industry = request.Industry
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

        public void Update(Guid id, ProductRequest request)
        {
            Product product = GetActive(id);

            product.Name = request.Name;
            product.Brand = request.Brand;
            product.Category = request.Category;
            product.Price = request.Price;
            product.Industry = request.Industry;
            _productRepository.Update(product);
        }

        // Baja lógica: también se da de baja su stock en todas las sucursales
        public void Delete(Guid id)
        {
            Product product = GetActive(id);

            _productRepository.Delete(product, _stockRepository.GetByProduct(id));
        }

        private static bool SameText(string a, string b)
        {
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
