using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace NDTField.Web.Controllers;

public class ErrorController : Controller
{
    [Route("Error/404")]
    public IActionResult NotFound404()
    {
        return View("NotFound");
    }

    [Route("Error/500")]
    public IActionResult ServerError()
    {
        var feature = HttpContext.Features
            .Get<IExceptionHandlerFeature>();
        return View("ServerError");
    }

    [Route("Error/{code}")]
    public IActionResult Index(int code)
    {
        return code switch
        {
            404 => View("NotFound"),
            _ => View("ServerError")
        };
    }
}