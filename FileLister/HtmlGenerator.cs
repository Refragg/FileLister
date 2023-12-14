using System.Text;
using System.Text.Encodings.Web;

namespace FileLister;

public class HtmlGenerator
{
    public static string GenerateViewerPage(string path, bool found, IEnumerable<FileInfo> files, IEnumerable<DirectoryInfo> directories)
    {
        StringBuilder stringBuilder = new StringBuilder();

        string titlePath = path == "" ? "Home" : "/" + path;
        
        stringBuilder.Append(
            $"""
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset="utf-8">
                    <title>{titlePath}{Program.TitleSuffix}</title>
                    <link rel="stylesheet" type="text/css" href="/{Program.WebsiteResourcesRoot}/breadcrumbs.css">
                    <link rel="stylesheet" type="text/css" href="/{Program.WebsiteResourcesRoot}/style.css">
                    <link rel="stylesheet" type="text/css" href="/{Program.WebsiteResourcesRoot}/table.css">
                </head>
                <body>
                    <div class="main">
                        <div class="header">
                            <a href="/">
                                <img src="/{Program.WebsiteResourcesRoot}/header-img.png" class="header-img"/>
                            </a>
                            <div class="navbar">
                                <nav class="breadcrumbs">
            """);

        string[] pathParts = path.Split('/');
        
        // Are we at the root?
        if (path == "")
            stringBuilder.Append("""<a href="/" class="breadcrumbs__item is-active">Home</a>""");
        else
        {
            stringBuilder.Append("""<a href="/" class="breadcrumbs__item">Home</a>""");
            
            for (int i = 0; i < pathParts.Length; i++)
            {
                string previousDirPath = "/";
                
                for (int j = 0; j <= i; j++)
                {
                    previousDirPath += pathParts[j] + "/";
                }
                
                // Make sure the last breadcrumbs item the active one
                if (i == pathParts.Length - 1)
                    stringBuilder.Append($"""<a href="{previousDirPath}" class="breadcrumbs__item is-active">{pathParts[i]}</a>""");    
                else
                    stringBuilder.Append($"""<a href="{previousDirPath}" class="breadcrumbs__item">{pathParts[i]}</a>""");
            }
        }

        stringBuilder.Append(
            """
                     </nav>
                 </div>
             </div>
            """);

        if (!found)
        {
            stringBuilder.Append(
                """
                <div class="not-found">
                    <h1>This file / directory does not exist</h1>
                """);
            return stringBuilder.ToString();
        }

        stringBuilder.Append(
            """
            <div class="files">
                <table class="files-table">
                    <thead>
                        <tr>
                            <th class="table-header-entry">Entry</th>
                            <th class="table-header-type">Type</th>
                            <th class="table-header-size">Size</th>
                        </tr>
                    </thead>
                    <tbody>
            """);
        
        // If we aren't at the root directory, add a '..' entry
        if (path != "")
        {
            string previousDirectoryPath = "/";
            
            for (int i = 0; i < pathParts.Length - 1; i++)
                previousDirectoryPath += pathParts[i] + "/";
            
            stringBuilder.Append(
                $"""
                 <tr>
                     <td><a href="{previousDirectoryPath}">..</a></td>
                     <td>Parent directory</td>
                     <td></td>
                 </tr>
                 """);
            
            // Kind of a hack but we need a slash at the beginning of our links
            // If we have an empty path, there will already be a slash inserted followed
            // by the name of the file / directory but that's not the case for non empty paths
            path = path.Insert(0, "/");
        }
        
        foreach (DirectoryInfo directory in directories)
        {
            Console.WriteLine("path: " + path + ", name: " + directory.Name);
            
            stringBuilder.Append(
                $"""
                 <tr>
                     <td><a href="{path}/{HtmlEncoder.Default.Encode(directory.Name)}">{directory.Name}</a></td>
                     <td>Directory</td>
                     <td></td>
                 </tr>
                 """);
        }
        
        foreach (FileInfo file in files)
        {
            stringBuilder.Append(
                $"""
                 <tr>
                     <td><a href="{path}/{HtmlEncoder.Default.Encode(file.Name)}">{file.Name}</a></td>
                     <td>{GetFileType(file)}</td>
                     <td>{GetFileSize(file)}</td>
                 </tr>
                 """);
        }

        return stringBuilder.ToString();
    }

    private static string GetFileType(FileInfo entry)
    {
        int indexOfLastDot = entry.Name.LastIndexOf('.');
        if (indexOfLastDot == -1)
            return "File";

        return entry.Name.Substring(indexOfLastDot + 1);
    }
    
    private static string GetFileSize(FileInfo entry)
    {
        return entry.Length switch
        {
            < 1_000 => $"{entry.Length} bytes",
            < 1_000_000 => $"{entry.Length / 1_000} KB",
            < 1_000_000_000 => $"{entry.Length / 1_000_000} MB",
            < 1_000_000_000_000 => $"{entry.Length / 1_000_000_000} GB",
            _ => $"{entry.Length / 1_000_000_000_000} TB"
        };
    }
}