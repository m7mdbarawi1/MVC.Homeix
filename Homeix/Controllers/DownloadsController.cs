using Microsoft.AspNetCore.Mvc;
using System.IO;

public class DownloadsController : Controller
{
    public IActionResult MobileApp()
    {
        // Find the file
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads", "mobile.apk");

        if (System.IO.File.Exists(filePath))
        {
            // Return the file for download
            return PhysicalFile(filePath, "application/vnd.android.package-archive", "Homeix.apk");
        }

        return NotFound("App file not found");
    }
}