using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace WebApplication1.Pages;

public class TestModel : PageModel
{
    public readonly Assembly Assembly = typeof(TestModel).Assembly;

    public int PropertyValue { get; set; }

    public void OnGet()
    {
        PropertyValue = Random.Shared.Next();
    }

    // setting?handler=DateTime
    public IActionResult OnGetDateTime() => Content("WebApplication1 " + DateTime.Now);
}