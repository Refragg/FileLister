using System.Net;
using System.Text;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace FileLister;

internal class Program
{
    public const string TitleSuffix = "TITLE_SUFFIX";
    
    public const string FilesRoot = "/Files/";

    public const string WebsiteResourcesRoot = "_website-resources";
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddTransient<FileBrowserMiddleware>();

        var app = builder.Build();

        app.UseMiddleware<FileBrowserMiddleware>();

        app.UseStaticFiles(new StaticFileOptions { RequestPath = "/" + WebsiteResourcesRoot });

        var contentTypeProvider = new FileExtensionContentTypeProvider();
        contentTypeProvider.Mappings[".7z"] = "application/x-7z-compressed";

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(FilesRoot, ExclusionFilters.None),
            ContentTypeProvider = contentTypeProvider,
            ServeUnknownFileTypes = true,
            RedirectToAppendTrailingSlash = true
        });
        
        app.Run();
    }
}

public class FileBrowserMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Trim trailing slashes
        context.Request.Path = context.Request.Path.ToString().TrimEnd('/');
        
        // Trim leading slashes separately because context.Request.Path requires a slash at the beginning
        string path = WebUtility.UrlDecode(context.Request.Path).TrimStart('/');

        // If the request is for a website resource, we let ASP NET Core do it's thing
        if (path.StartsWith(Program.WebsiteResourcesRoot))
            return next(context);
        
        context.Response.Clear();
        context.Response.ContentType = "text/html; charset=utf-8";

        string fullPath = Path.Combine(Program.FilesRoot, path);

        if (!Directory.Exists(fullPath))
        {
            // If we found the file, we let ASP NET Core serve it
            if (File.Exists(fullPath))
                return next(context);
            
            // Otherwise, there's nothing at this path, show the not found page
            byte[] bytesNotFound = Encoding.UTF8.GetBytes(HtmlGenerator.GenerateViewerPage(path, false, Enumerable.Empty<FileInfo>(), Enumerable.Empty<DirectoryInfo>()));
            context.Response.ContentLength = bytesNotFound.Length;
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return context.Response.Body.WriteAsync(bytesNotFound, 0, bytesNotFound.Length);
        }
        
        DirectoryInfo directory = new DirectoryInfo(fullPath);
        IEnumerable<FileInfo> files = directory.EnumerateFiles().OrderBy(x => x.Name.ToUpper());
        IEnumerable<DirectoryInfo> directories = directory.EnumerateDirectories().OrderBy(x => x.Name.ToUpper());
        
        byte[] bytes = Encoding.UTF8.GetBytes(HtmlGenerator.GenerateViewerPage(path, true, files, directories));
        context.Response.ContentLength = bytes.Length;
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        return context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
}