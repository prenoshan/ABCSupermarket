using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ABCSupermarket.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;

namespace ABCSupermarket.Controllers
{
    public class ProductsController : Controller
    {

        //context reference to the ABC supermarket database
        private readonly ABCSupermarketContext _context;

        //constructor that sets the context of ABC supermarket
        public ProductsController(ABCSupermarketContext context)
        {
            _context = context;
        }

        //get request for the index page
        public async Task<IActionResult> Index(string productName)
        {

            //linq query to read all the products from the database
            var products = from p in _context.Product
                           select p;

            //returns a product based on the name the user searches for
            if (!String.IsNullOrEmpty(productName))
            {

                products = products.Where(s => s.Name.Contains(productName));

            }

            return View(await products.ToListAsync());
        }

        //get request for the details page 
        public async Task<IActionResult> Details(int? id)
        {

            //returns a 404 request if id does not match an existing product id
            if (id == null)
            {
                return NotFound();
            }

            //linq query to return a product based on the id that is set when the user views a product's details
            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ID == id);

            //returns a 404 request if no product exists
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //get request for the create page
        public IActionResult Create()
        {

            //creates a viewdata object to be used to validate the image uploaded
            ViewData["FileError"] = "";

            return View();
        }

        //returns a blob container from azure blob storage
        private CloudBlobContainer GetCloudBlobContainer()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot Configuration = builder.Build();
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(Configuration["ConnectionStrings:AzureStorageConnectionString-1"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("products");
            return container;
        }

        //returns the content type of the file a user uploads
        public string getMimeType(string filename)
        {

            var provider = new FileExtensionContentTypeProvider();

            string contentType;

            //gets the content type of the file that a user chooses
            if (!provider.TryGetContentType(filename, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;

        }

        //method to upload an image blob
        public async Task uploadImageBlobAsync(Product product, IFormFile file)
        {

            //gets the blob container to reference
            CloudBlobContainer container = GetCloudBlobContainer();

            //stores the image file name of the image to be uploaded
            string imageToUpload = file.FileName;

            //creates a unique blob reference for each image uploaded
            CloudBlockBlob blob = container.GetBlockBlobReference(Guid.NewGuid().ToString());

            //sets the content type of the image that is being uploaded
            blob.Properties.ContentType = getMimeType(imageToUpload);

            //async method to upload the image
            await blob.UploadFromStreamAsync(file.OpenReadStream());

            //once the image is uploaded return the uri of the image
            var imageURL = blob.Uri.AbsoluteUri;

            //set the image url for the model equal to the uri from the uploaded image blob
            product.ImageURL = imageURL;

        }

        //method to delete an image blob
        public async Task deleteBlobAsync(Product product)
        {

            CloudBlobContainer container = GetCloudBlobContainer();

            //parses the image url as a uri
            Uri imageURI = new Uri(product.ImageURL);

            //gets the blob reference of the uri in the blob container
            CloudBlockBlob blob = container.GetBlockBlobReference(new CloudBlockBlob(imageURI).Name);

            //deletes the blob
            await blob.DeleteAsync();

        }

        //handles the post request for the create action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Description,Price")] Product product)
        {

            //if all model fields are populated
            if (ModelState.IsValid)
            {

                //loops through all files in the create view when the form is posted
                foreach (var file in Request.Form.Files)
                {

                    //validation to make sure a file is selected
                    if (file != null && file.Length > 0)
                    {
                        //checks if the file the user selects is not a valid image
                        if (!file.ContentType.Contains("image"))
                        {
                            //populates the viewdata object to display an error on the view
                            ViewData["FileError"] = "Please select a valid image";

                            return View(product);

                        }

                        else
                        {

                            //calls the method to upload an image blob
                            await uploadImageBlobAsync(product, file);

                            //adds the product entry to the database
                            _context.Add(product);
                            await _context.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }

                    }

                }

                //if no file is selected, the viewdata object is updated with the relevant error message
                ViewData["FileError"] = "Please select an image";

            }
            return View(product);
        }

        //get request for the edit page 
        public async Task<IActionResult> Edit(int? id)
        {

            //creates a viewdata object to be used to validate the image uploaded
            ViewData["FileError"] = "";

            //returns a 404 request if id does not match an existing product id
            if (id == null)
            {
                return NotFound();
            }

            //returns a product based on the id that is set when the user edits a product
            var product = await _context.Product.FindAsync(id);

            //returns a 404 request if no product exists
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //handles the post request for the edit action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Description,Price,ImageURL")] Product product)
        {

            //returns a 404 request if id does not match an existing product id
            if (id != product.ID)
            {
                return NotFound();
            }

            //if all model fields are populated
            if (ModelState.IsValid)
            {
                try
                {

                    //loops through all files in the create view when the form is posted
                    foreach (var file in Request.Form.Files)
                    {
                        //validation that checks that a file a user chooses is not null
                        if (file != null || file.Length > 0)
                        {
                            //checks if the file the user selects is not a valid image
                            if (!file.ContentType.Contains("image"))
                            {
                                //populates the viewdata object to display an error on the view
                                ViewData["FileError"] = "Please select a valid image";

                                return View(product);

                            }

                            else
                            {

                                //deletes the current image blob for the product
                                await deleteBlobAsync(product);

                                //uploads a new image blob to the container that a user chooses 
                                await uploadImageBlobAsync(product, file);

                                //method that updates the product information in the database
                                _context.Update(product);

                                await _context.SaveChangesAsync();

                                return RedirectToAction(nameof(Index));
                            }
                        }

                    }

                    //updates only the required fields if a user chooses to not edit the image
                    _context.Update(product);

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));


                }

                //exception handling that returns a 404 request when there is no product found
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(product);
        }

        //get request for the delete page
        public async Task<IActionResult> Delete(int? id)
        {

            //returns a 404 request if id does not match an existing product id
            if (id == null)
            {
                return NotFound();
            }

            //returns a product based on the id that is set when the user wants to delete a product
            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ID == id);

            //returns a 404 request if no product exists
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            _context.Product.Remove(product);

            //method call to delete the image blob for the related product id
            await deleteBlobAsync(product);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ID == id);
        }
    }
}
