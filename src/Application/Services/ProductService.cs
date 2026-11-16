using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBranchStockRepository _stockRepository;
        private readonly AccessService _accessService;

        public ProductService(
            IProductRepository productRepository,
            IBranchStockRepository stockRepository,
            AccessService accessService)
        {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _accessService = accessService;
        }

        // Catálogo general de productos (se usa para dar de alta stock en una sucursal)
        public List<Product> GetAll()
        {
            return _productRepository.GetAll();
        }

        public Product GetById(Guid id)
        {
            Product? product = _productRepository.GetById(id);
            if (product is null)
            {
                throw new NotFoundException("El producto no existe.");
            }

            return product;
        }

        public Product Create(Product newProduct)
        {
            Validate(newProduct);

            newProduct.Id = Guid.NewGuid();
            newProduct.Name = newProduct.Name.Trim();
            newProduct.Brand = newProduct.Brand.Trim();
            newProduct.Category = newProduct.Category.Trim();

            return _productRepository.Create(newProduct);
        }

        // Solo se puede editar un producto que tenga stock en alguna sucursal del dueño
        public void Update(string cuit, Guid id, Product updatedProduct)
        {
            Validate(updatedProduct);
            Product product = GetOwnedProduct(cuit, id);

            product.Name = updatedProduct.Name.Trim();
            product.Brand = updatedProduct.Brand.Trim();
            product.Category = updatedProduct.Category.Trim();
            product.Price = updatedProduct.Price;
            product.Industry = updatedProduct.Industry;

            _productRepository.Update(product);
        }

        // Borrado físico: solo si el producto nunca se vendió ni se compró, para no perder el historial
        public void Delete(string cuit, Guid id)
        {
            Product product = GetOwnedProduct(cuit, id);

            if (_productRepository.HasMovements(id))
            {
                throw new AppValidationException("El producto tiene ventas o compras registradas: dalo de baja del stock de la sucursal en lugar de eliminarlo.");
            }

            _productRepository.Delete(product);
        }

        private Product GetOwnedProduct(string cuit, Guid id)
        {
            Product product = GetById(id);

            bool inMyBranches = _accessService.GetBranches(cuit)
                .Any(b => _stockRepository.GetByBranch(b.Id).Any(s => s.ProductId == id));
            if (!inMyBranches)
            {
                throw new NotAllowedException("El producto no está cargado en ninguna de tus sucursales.");
            }

            return product;
        }

        private static void Validate(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrWhiteSpace(product.Brand) || string.IsNullOrWhiteSpace(product.Category))
            {
                throw new AppValidationException("El nombre, la marca y la categoría del producto son obligatorios.");
            }

            if (product.Price < 0)
            {
                throw new AppValidationException("El precio debe ser mayor o igual a 0.");
            }
        }
    }
}
