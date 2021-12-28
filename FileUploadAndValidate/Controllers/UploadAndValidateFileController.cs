using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
namespace FileUploadAndValidate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadAndValidateFileController : Controller
    {
        /// <summary>
        /// Used to Upload and Vlidate text file and content
        /// </summary>
        /// <param name="file">Required to pass this parameter for testing with swagger</param>

        /// <returns></returns>
        [HttpPost("upload", Name = "upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {

            var files = Request.Form.Files;//This required while testing the code with Postman tool to pass the parameter file

            file = files[0];

            if (CheckIfTextFile(file))
            {

                bool isSaveSuccess;//= true;

                isSaveSuccess = await WriteFile(file);
                if (isSaveSuccess == true)
                {
                    ArrayList invalidRecords = new ArrayList();
                    try
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles", file.FileName);
                        string[] lines = System.IO.File.ReadAllLines(path);

                        int lineCount = 1;


                        bool isNameValid;
                        bool isAccountNumberValid;
                        bool isInvalidRequestFound = false;

                        string accountNumber;
                        string firstName;

                        foreach (string line in lines)
                        {
                            isNameValid = true;
                            isAccountNumberValid = true;


                            accountNumber = line.Split(' ')[1];
                            firstName = line.Split(' ')[0];


                            //For First Name testing
                            if (!Regex.Match(firstName, "^[A-Z][a-zA-Z]*$").Success)
                            {
                                isInvalidRequestFound = true;
                                isNameValid = false;
                            }


                            //For Account number testing
                            if (accountNumber.Length == 7)
                            {
                                if (!Regex.Match(accountNumber, "[3,4]\\d{6}$").Success)
                                {
                                    isInvalidRequestFound = true;
                                    isAccountNumberValid = false;
                                }
                            }
                            else if (accountNumber.Length > 8 || (accountNumber.Length == 8 && !Regex.Match(accountNumber, "[3,4]\\d{6}\\/?[p]").Success))
                            {

                                isInvalidRequestFound = true;
                                isAccountNumberValid = false;

                            }



                            if (!isNameValid && !isAccountNumberValid)
                            {
                                invalidRecords.Add("Account name, account number -not valid for " + lineCount + " line " + line + "");
                            }
                            else if (!isNameValid)
                            {
                                invalidRecords.Add("Account name -not valid for " + lineCount + " line " + line + "");
                            }
                            else if (!isAccountNumberValid)
                            {
                                invalidRecords.Add("Account number -not valid for " + lineCount + " line " + line + "");
                            }

                            lineCount++;

                        }

                        if (isInvalidRequestFound)
                        {
                            return Json(new { Message = "fileValid: false", invalidLines = invalidRecords });
                        }
                        else
                        {

                            return Json(new { Message = "fileValid: true" });
                        }

                    }
                    catch (Exception ex)
                    {
                      
                        return BadRequest(new { message = ex.Message + " Data from the file must be in format like 'FirstName AccountNumber'" });
                    }

                    
                }
                else
                {
                    return BadRequest(new { message = "Error while uploading file." });
                }

            }
            else
            {
                return BadRequest(new { message = "Invalid file extension. Only .txt file allowed to upload" });
            }

           
        }






        /// <summary>
        /// Method to check if file is excel file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool CheckIfTextFile(IFormFile file)
        {
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            return (extension == ".txt"); // Change the extension based on your need


        }


        /// <summary>
        /// Used to upload file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<bool> WriteFile(IFormFile file)
        {
            bool isSaveSuccess = false;

            try
            {
                var pathBuilt = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

                if (!Directory.Exists(pathBuilt))
                {
                    Directory.CreateDirectory(pathBuilt);
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles", file.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                isSaveSuccess = true;
            }
            catch
            {
                //log error
            }

            return isSaveSuccess;


        }
    }
}
