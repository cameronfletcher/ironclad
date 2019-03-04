namespace SampleWebApi
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    // decorating this class with the Authorize attribute will require the user to be authenticated before any method gets invoked
    [Authorize]
    public class SampleController : ControllerBase
    {
        [HttpGet("api")]
        public IActionResult Success()
        {
            // this is an example of pulling out the 'user' from the context of the request
            // the 'user' *will* exist as authentication has taken place; however the 'user' may be a client and not an end-user based on the permission you have set
            return this.Ok(new { message = $"Hello {this.User.Identity.Name ?? "{{unknown}}"}" });
        }

        // if the user is authenticated but is trying to do something that they do not have permission to do then we should forbid them
        [HttpGet("forbidden")]
        public IActionResult Failure() => this.Forbid();

        // this is an example of a call that is locked down to a 'policy' specified in StartUp.cs restricting calls to the role of 'admin'
        [Authorize("admin_policy")]
        [HttpGet("admin")]
        public IActionResult Admin()
        {
            return this.Ok(new { message = $"Hello {this.User.Identity.Name ?? "{{unknown}}"} (admin)" });
        }
    }
}
