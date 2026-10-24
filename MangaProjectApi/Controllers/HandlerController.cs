using Microsoft.AspNetCore.Mvc;

namespace MangaProjectApi.Controllers
{
    public class HandlerController: Controller
    {
        [NonAction]
        public IActionResult ResponseHandler(dynamic _class)
        {
            return ResponseHandler(_class, _class.Status);
            //return _class.Status switch
            //{
            //    200 => (IActionResult)Ok(_class),
            //    201 => (IActionResult)StatusCode(StatusCodes.Status201Created),
            //    400 => (IActionResult)StatusCode(StatusCodes.Status400BadRequest, _class),
            //    403 => (IActionResult)StatusCode(StatusCodes.Status403Forbidden, _class),
            //    404 => (IActionResult)NotFound(_class),
            //    409 => (IActionResult)Conflict(_class),
            //    _ => StatusCode(StatusCodes.Status500InternalServerError),
            //};
        }

        [NonAction]
        public IActionResult ResponseHandler(dynamic? _class, int Status)
        {
            return Status switch
            {
                200 => (IActionResult)Ok(_class),
                201 => StatusCode(StatusCodes.Status201Created),
                400 => (IActionResult)StatusCode(StatusCodes.Status400BadRequest, _class),
                403 => (IActionResult)StatusCode(StatusCodes.Status403Forbidden, _class),
                404 => (IActionResult)NotFound(_class),
                409 => (IActionResult)Conflict(_class),
                _ => StatusCode(StatusCodes.Status500InternalServerError),
            };
        }
    }
}
