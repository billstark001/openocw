using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oocw.Backend.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Oocw.Backend.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class WebController : Controller
{
    private static readonly Dictionary<string, string> _pageCache = [];

    private readonly string _indexPath;

    public WebController(
        IWebHostEnvironment env
        )
    {
        _indexPath = Path.Combine(env.WebRootPath, "index.html");
    }

    [HttpGet("/")]
    public async Task<ActionResult<string>> GetIndex()
    {
        try
        {
            var succ = _pageCache.TryGetValue(_indexPath, out var txt);
            if (!succ)
            {
                txt = await System.IO.File.ReadAllTextAsync(_indexPath, Encoding.UTF8);
                _pageCache[_indexPath] = txt;
            }
            if (txt == null)
                throw new NotImplementedException();
            Response.ContentType = "text/html; charset=utf-8";
            await Response.WriteAsync(txt);
            return new EmptyResult();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Redirect("/index.html");
        }

    }

    [HttpGet("/api/root")]
    public ApiResult GetApiRoot()
    {
        return new(Definitions.CODE_SUCC);
    }
}

