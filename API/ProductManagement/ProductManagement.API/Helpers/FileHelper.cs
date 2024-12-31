using Microsoft.AspNetCore.Mvc;

using ProductManagement.API.ResponseModule;

namespace ProductManagement.API.Helpers;

public static class FileHelper
{
    public static HttpContext HttpContext => new HttpContextAccessor().HttpContext!;
    public static IWebHostEnvironment WebHostEnvironment => (IWebHostEnvironment)HttpContext!.RequestServices!.GetService(typeof(IWebHostEnvironment))!;

    // Start Single File Operations //
    public static IActionResult GetFile(ControllerBase controller, string entityName, string entityId, HttpRequest httpRequest)
    {
        var folderPath = GetFolderPath(entityName, entityId);

        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var fileInfo = directoryInfo.GetFiles();
            var filename = fileInfo[0].Name;

            var filePath = folderPath + "\\" + filename;
            if (File.Exists(filePath))
            {
                //var fileUrl = hostUrl + "/Upload/" + entityName + "/" + entityId + "/" + filename;
                var fileUrl = entityName + "/" + entityId + "/" + filename;
                return controller.Ok(fileUrl);
            }
            else
                return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
        }
        else
            return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
    }

    public static async Task<IActionResult> UploadFile(ControllerBase controller, IFormFile formFile, string entityName, string entityId, bool isPdf = false)
    {
        if (isPdf)
        {
            if (formFile.ContentType != "application/pdf")
                return controller.Ok(new ApiResponse(400, "Invalid File only .pdf is allowed "));
        }
        else
        {
            if (formFile.ContentType != "image/jpeg" && formFile.ContentType != "image/png") // add webm?
                return controller.Ok(new ApiResponse(400, "Invalid File only .jpg, .jpeg, and png is allowed "));
        }

        var folderPath = GetFolderPath(entityName, entityId);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var guid = Guid.NewGuid();
        var filePath = folderPath + "\\" + guid;
        if (isPdf)
            filePath += ".pdf";
        else
            filePath += ".png";

        var directoryInfo = new DirectoryInfo(folderPath);
        var fileInfo = directoryInfo.GetFiles();
        var postFilePath = string.Empty;
        if (fileInfo.Length != 0)
        {
            var fileName = fileInfo[0].Name;
            postFilePath = folderPath + "\\" + fileName;
        }

        if (fileInfo.Length >= 1 && !File.Exists(postFilePath))
            return controller.Ok(new ApiResponse(400, $"The {entityName} with id [{entityId}] already have File"));
        else if (fileInfo.Length == 1 && File.Exists(postFilePath))
            File.Delete(postFilePath);

        using var stream = File.Create(filePath);
        await formFile.CopyToAsync(stream);
        stream.Close();

        if (isPdf)
        {
            var validPDF = PdfHelper.ValidatePdf(filePath);
            if (!validPDF)
            {
                File.Delete(filePath);
                return controller.Ok(new ApiResponse(400, "Invalid PDF File"));
            }
        }
        else
        {
            var validIMG = ImageHelper.ValidateImage(filePath);
            if (!validIMG)
            {
                File.Delete(filePath);
                return controller.Ok(new ApiResponse(400, "Invalid Image File"));
            }
            //else
            //{
            //    ImageHelper.ResizeImage(filePath);
            //}
        }
        return controller.Ok(new ApiResponse(200, $"File for the {entityName} [{entityId}] is uploaded successfully."));
    }

    public static async Task<IActionResult> UploadCsvFile(ControllerBase controller, IFormFile formFile, string entityName, string entityId)
    {
        if (formFile.ContentType != "text/csv" && formFile.ContentType != "application/vnd.ms-excel")
            return controller.Ok(new ApiResponse(400, "Invalid File. Only .csv files are allowed."));

        var folderPath = GetFolderPath(entityName, entityId);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var guid = Guid.NewGuid();
        var filePath = folderPath + "\\" + guid;
        filePath += ".csv";


        var directoryInfo = new DirectoryInfo(folderPath);
        var fileInfo = directoryInfo.GetFiles();
        var postFilePath = string.Empty;
        if (fileInfo.Length != 0)
        {
            var fileName = fileInfo[0].Name;
            postFilePath = folderPath + "\\" + fileName;
        }

        if (fileInfo.Length >= 1 && !File.Exists(postFilePath))
            return controller.Ok(new ApiResponse(400, $"The {entityName} with id [{entityId}] already have File"));
        else if (fileInfo.Length == 1 && File.Exists(postFilePath))
            File.Delete(postFilePath);

        using var stream = File.Create(filePath);
        await formFile.CopyToAsync(stream);
        stream.Close();

        return controller.Ok(new ApiResponse(200, $"File for the {entityName} [{entityId}] is uploaded successfully."));
    }

    public static async Task<IActionResult> DownloadFile(ControllerBase controller, string entityName, string entityId, bool isPdf = false)
    {
        var folderPath = GetFolderPath(entityName, entityId);

        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var fileInfo = directoryInfo.GetFiles();
            var filename = fileInfo[0].Name;
            var filePath = folderPath + "\\" + filename;
            if (File.Exists(filePath))
            {
                var stream = new MemoryStream();
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    await fileStream.CopyToAsync(stream);
                }
                stream.Position = 0;

                if (isPdf)
                    return controller.File(stream, "application/pdf", filename);
                else
                    return controller.File(stream, "image/png", filename);
            }
            else
                return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
        }
        else
            return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
    }

    public static IActionResult RemoveFile(ControllerBase controller, string entityName, string entityId)
    {
        var folderPath = GetFolderPath(entityName, entityId);

        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var fileInfo = directoryInfo.GetFiles();
            var filename = fileInfo[0].Name;
            var filePath = folderPath + "\\" + filename;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Directory.Delete(folderPath);
                return controller.Ok(new ApiResponse(200, $"File for the {entityName} [{entityId}] is deleted successfully."));
            }
            else
                return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
        }
        else
            return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
    }

    public static IActionResult RemoveSingleFile(ControllerBase controller, string entityName, string entityId, string fileGuid)
    {
        var folderPath = GetFolderPath(entityName, entityId);

        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var fileInfo = directoryInfo.GetFiles();
            var filename = string.Empty;
            foreach (var file in fileInfo)
                if (file.Name == fileGuid)
                    filename = file.Name;

            var filePath = folderPath + "\\" + filename;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return controller.Ok(new ApiResponse(200, $"File for the {entityName} [{entityId}] is deleted successfully."));
            }
            else
                return controller.NotFound(new ApiResponse(404, $"File with the name {fileGuid} isn't found"));
        }
        else
        {
            return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
        }
    }

    // End Single File Operations //

    // Start Multiple File Operations //
    public static IActionResult GetMultipleFiles(ControllerBase controller, string entityName, string entityId, HttpRequest httpRequest)
    {
        var folderPath = GetFolderPath(entityName, entityId);

        if (Directory.Exists(folderPath))
        {
            var fileUrl = new List<string>();

            var directoryInfo = new DirectoryInfo(folderPath);
            var fileInfos = directoryInfo.GetFiles();
            foreach (var file in fileInfos)
            {
                var filename = file.Name;
                var filePath = folderPath + "\\" + filename;
                if (File.Exists(filePath))
                {
                    var _fileUrl = entityName + "/" + entityId + "/" + filename;
                    fileUrl.Add(_fileUrl);
                }
            }
            return controller.Ok(fileUrl);
        }
        else
            return controller.NotFound(new ApiResponse(404, $"No files found for {entityName} with id [{entityId}]."));
    }

    public static List<string>? GetMultipleFiles(string entityName, string? entityId, List<string>? entitiesIds)
    {
        if (entityId != null)
        {
            var folderPath = GetFolderPath(entityName, entityId);

            if (Directory.Exists(folderPath))
            {
                var fileUrl = new List<string>();

                var directoryInfo = new DirectoryInfo(folderPath);
                var fileInfos = directoryInfo.GetFiles();
                foreach (var file in fileInfos)
                {
                    var filename = file.Name;
                    var filePath = Path.Combine(folderPath, filename);
                    if (File.Exists(filePath))
                    {
                        var _fileUrl = entityName + "/" + entityId + "/" + filename;
                        fileUrl.Add(_fileUrl);
                    }
                }
                return fileUrl;
            }
        }
        else if (entitiesIds != null && entitiesIds.Count > 0)
        {
            var fileUrls = new List<string>();

            foreach (var entityIdValue in entitiesIds)
            {
                var folderPath = GetFolderPath(entityName, entityIdValue);

                if (Directory.Exists(folderPath))
                {
                    var directoryInfo = new DirectoryInfo(folderPath);
                    var fileInfos = directoryInfo.GetFiles();
                    foreach (var file in fileInfos)
                    {
                        var filename = file.Name;
                        var filePath = Path.Combine(folderPath, filename);
                        if (File.Exists(filePath))
                        {
                            var _fileUrl = entityName + "/" + entityIdValue + "/" + filename;
                            fileUrls.Add(_fileUrl);
                        }
                    }
                }
            }
            return fileUrls;
        }

        return null;
    }


    public static async Task<IActionResult> UploadMultipleFiles(ControllerBase controller, List<IFormFile> formFileList, string entityName, string entityId, bool isPdf = false)
    {
        if (isPdf)
        {
            foreach (var file in formFileList)
                if (file.ContentType != "application/pdf")
                    return controller.Ok(new ApiResponse(400, "Invalid Files only .pdf is allowed "));
        }
        else
        {
            foreach (var file in formFileList)
                if (file.ContentType != "image/jpeg" && file.ContentType != "image/png") // add webm?
                    return controller.Ok(new ApiResponse(400, "Invalid Files only .jpg, .jpeg, and png is allowed "));
        }

        var folderPath = GetFolderPath(entityName, entityId);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var failedFiles = 0;
        foreach (var file in formFileList)
        {
            var guid = Guid.NewGuid();
            var filePath = string.Empty;
            if (isPdf)
            {
                var fileNameWithoutExtension = file.FileName.Replace(".pdf", "");
                filePath = folderPath + "\\" + fileNameWithoutExtension + "_" + guid;
                filePath += ".pdf";
            }
            else
            {
                var fileNameWithoutExtension = file.FileName.Replace(".png", "")
                    .Replace(".jpg", "")
                    .Replace(".jpeg", "");

                filePath = folderPath + "\\" + fileNameWithoutExtension + "_" + guid;
                filePath += ".png";
            }

            if (File.Exists(filePath))
                File.Delete(filePath);

            using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);
            stream.Close();

            if (isPdf)
            {
                var validPDF = PdfHelper.ValidatePdf(filePath);
                if (!validPDF)
                {
                    File.Delete(filePath);
                    failedFiles++;
                }
            }
            else
            {
                var validIMG = ImageHelper.ValidateImage(filePath);
                if (!validIMG)
                {
                    File.Delete(filePath);
                    failedFiles++;
                }
                //else
                //{
                //    ImageHelper.ResizeImage(filePath);
                //}
            }
        }
        var uploadedFiles = formFileList.Count() - failedFiles;
        return controller.Ok(new ApiResponse(200, $"{uploadedFiles} Files for the {entityName} [{entityId}] is uploaded successfully, while {failedFiles} Files failed."));
    }

    public static async Task<IActionResult> UploadMixedMultipleFiles(ControllerBase controller, List<IFormFile> formFileList, string entityName, string entityId)
    {
        foreach (var file in formFileList)
            if (file.ContentType != "image/jpeg" && file.ContentType != "image/png" && file.ContentType != "application/pdf")
                return controller.Ok(new ApiResponse(400, "Invalid Files only .jpg, .jpeg, .png and .pdf is allowed "));

        var folderPath = GetFolderPath(entityName, entityId);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var failedFiles = 0;
        foreach (var file in formFileList)
        {
            var guid = Guid.NewGuid();
            var filePath = string.Empty;
            if (file.ContentType == "application/pdf")
            {
                var fileNameWithoutExtension = file.FileName.Replace(".pdf", "");
                filePath = folderPath + "\\" + fileNameWithoutExtension + "_" + guid;
                filePath += ".pdf";
            }
            else
            {
                var fileNameWithoutExtension = file.FileName.Replace(".png", "")
                    .Replace(".jpg", "")
                    .Replace(".jpeg", "");

                filePath = folderPath + "\\" + fileNameWithoutExtension + "_" + guid;
                filePath += ".png";
            }

            if (File.Exists(filePath))
                File.Delete(filePath);

            using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);
            stream.Close();

            if (file.ContentType == "application/pdf")
            {
                var validPDF = PdfHelper.ValidatePdf(filePath);
                if (!validPDF)
                {
                    File.Delete(filePath);
                    failedFiles++;
                }
            }
            else
            {
                var validIMG = ImageHelper.ValidateImage(filePath);
                if (!validIMG)
                {
                    File.Delete(filePath);
                    failedFiles++;
                }
                //else
                //{
                //    ImageHelper.ResizeImage(filePath);
                //}
            }
        }
        var uploadedFiles = formFileList.Count() - failedFiles;
        return controller.Ok(new ApiResponse(200, $"{uploadedFiles} Files for the {entityName} [{entityId}] is uploaded successfully, while {failedFiles} Files failed."));
    }

    public static async Task<IActionResult> UploadMixedMultipleFilesGuid(ControllerBase controller, List<IFormFile> formFileList, string entityName, string entityId)
    {
        if (formFileList.Count == 0)
            return controller.Ok(new ApiResponse(400, "No Files Chosen"));

        foreach (var file in formFileList)
        {
            if (file.ContentType != "image/jpeg" && file.ContentType != "image/png" && file.ContentType != "application/pdf")
                return controller.Ok(new ApiResponse(400, "Invalid Files only .jpg, .jpeg, .png and .pdf is allowed "));
        }

        var folderPath = GetFolderPath(entityName, entityId);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var failedFiles = 0;
        foreach (var file in formFileList)
        {
            var guid = Guid.NewGuid();
            var filePath = string.Empty;
            if (file.ContentType == "application/pdf")
            {
                filePath = folderPath + "\\" + guid;
                filePath += ".pdf";
            }
            else
            {
                filePath = folderPath + "\\" + guid;
                filePath += ".png";
            }

            if (File.Exists(filePath))
                File.Delete(filePath);

            using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);
            stream.Close();

            if (file.ContentType == "application/pdf")
            {
                var validPDF = PdfHelper.ValidatePdf(filePath);
                if (!validPDF)
                {
                    File.Delete(filePath);
                    failedFiles++;
                }
            }
            else
            {
                var validIMG = ImageHelper.ValidateImage(filePath);
                if (!validIMG)
                {
                    File.Delete(filePath);
                    failedFiles++;
                }
                //else
                //{
                //    ImageHelper.ResizeImage(filePath);
                //}
            }
        }
        var uploadedFiles = formFileList.Count() - failedFiles;
        return controller.Ok(new ApiResponse(200, $"{uploadedFiles} Files for the {entityName} [{entityId}] is uploaded successfully, while {failedFiles} Files failed."));
    }

    public static IActionResult RemoveAllFilesInFolder(ControllerBase controller, string entityName, string entityId)
    {
        var folderPath = GetFolderPath(entityName, entityId);

        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var fileInfo = directoryInfo.GetFiles();
            foreach (var file in fileInfo)
            {
                file.Delete();
            }
            Directory.Delete(folderPath);
            return controller.Ok(new ApiResponse(200, $"Files for the {entityName} [{entityId}] is deleted successfully."));
        }
        else
            return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
    }

    public static IActionResult RemoveSelectedFilesInFolder(ControllerBase controller, string entityName, string entityId, List<string> filesGuid)
    {
        var folderPath = GetFolderPath(entityName, entityId);

        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var fileInfo = directoryInfo.GetFiles();
            foreach (var fileGuid in filesGuid)
            {
                var matchingFile = Array.Find(fileInfo, x => x.Name == fileGuid);
                matchingFile?.Delete();
            }
            return controller.Ok(new ApiResponse(200, $"Selected files for the {entityName} [{entityId}] is deleted successfully."));
        }
        else
            return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
    }

    public static IActionResult RemoveSingleSelectedFileInFolder(ControllerBase controller, string entityName, string entityId, string fileGuid)
    {
        var folderPath = GetFolderPath(entityName, entityId);

        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var fileInfo = directoryInfo.GetFiles();
            var matchingFile = Array.Find(fileInfo, x => x.Name == fileGuid);
            if (matchingFile != null)
            {
                matchingFile.Delete();
                return controller.Ok(new ApiResponse(200, $"Selected file for the {entityName} [{entityId}] is deleted successfully."));
            }
            else
                return controller.Ok(new ApiResponse(400, $"File {fileGuid} in {entityName} [{entityId}] isn't found."));
        }
        else
            return controller.NotFound(new ApiResponse(404, $"{entityName} with id [{entityId}] isn't found."));
    }
    // End Multiple File Operations //

    // Start General Operations //
    public static bool IsFileExist(string entityName, string entityId)
    {
        var folderPath = GetFolderPath(entityName, entityId);
        return Directory.Exists(folderPath);
    }

    public static string GetFileUrl(string entityName, string entityId, string defaultUrl)
    {
        var folderPath = GetFolderPath(entityName, entityId);

        if (Directory.Exists(folderPath))
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var fileInfo = directoryInfo.GetFiles();
            var filename = fileInfo[0].Name;

            var filePath = folderPath + "\\" + filename;
            if (File.Exists(filePath))
                return entityName + "/" + entityId + "/" + filename;
            else
                return defaultUrl;
        }
        else
            return defaultUrl;
    }

    private static string GetFolderPath(string entityName, string entityId)
    {
        var mainFolder = "Upload";
        return WebHostEnvironment.WebRootPath + "\\" + mainFolder + "\\" + entityName + "\\" + entityId;
    }
    // End General Operations //
}
