﻿using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace CorteImagenes.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public ActionResult CortarImagenPdf()
        {
            try
            {
                var file = Request.Files[0];
                var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                var obj = (HttpPostedFile)constructorInfo
                          .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });


                HttpPostedFile filePosted = obj;
                string filePath = "";
                if (filePosted != null && filePosted.ContentLength > 0)
                {
                    string fileNameApplication = System.IO.Path.GetFileName(filePosted.FileName);
                    string fileExtensionApplication = System.IO.Path.GetExtension(fileNameApplication);

                    // generating a random guid for a new file at server for the uploaded file
                    string newFile = Guid.NewGuid().ToString() + fileExtensionApplication;
                    // getting a valid server path to save

                    filePath = System.IO.Path.Combine(@"D:/PdfToImages", newFile);

                    if (fileNameApplication != String.Empty)
                    {
                        filePosted.SaveAs(filePath);
                    }
                }

                var xDpi = 192; //set the x DPI
                var yDpi = 192; //set the y DPI



                using (var rasterizer = new GhostscriptRasterizer()) //create an instance for GhostscriptRasterizer
                {
                    rasterizer.Open(filePath); //opens the PDF file for rasterizing
                    int frameNum = rasterizer.PageCount;

                    //set the output image(png's) complete path
                    var outputPNGPath = Path.Combine(filePath, string.Format("{0}.png", @"D:/PdfToImages"));

                    //converts the PDF pages to png's 
                    for (int i = 1; i < frameNum; i++)
                    {

                        var pdf2PNG = rasterizer.GetPage(xDpi, yDpi, i);

                        //save the png's
                        pdf2PNG.Save("D:/PdfToImages" + "\\" + Convert.ToString(i) + ".jpg", ImageFormat.Jpeg);
                        //pdf2PNG.Save("C:/PdfToImages", ImageFormat.Png);
                    }

                    rasterizer.Close();
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.AddDirectory(@"D:/PdfToImages");
                        zip.Save(@"D:/PdfToImages/PdfToImages.zip");
                    }
                    Byte[] bytes = System.IO.File.ReadAllBytes(@"D:/PdfToImages/PdfToImages.zip");
                    var directorio = new DirectoryInfo(@"D:/PdfToImages");
                    var archivos = directorio.GetFiles();
                    foreach (var archivo in archivos)
                    {
                        System.IO.File.Delete(archivo.FullName);
                    }

                    //return RedirectToAction("Index");
                    return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "Pdf_images.zip");
                }
            }
            catch (Exception)
            {
                var directorio = new DirectoryInfo(@"D:/PdfToImages");
                var archivos = directorio.GetFiles();
                foreach (var archivo in archivos)
                {
                    System.IO.File.Delete(archivo.FullName);
                }
                return RedirectToAction("Index");
            }

        }
    }
}