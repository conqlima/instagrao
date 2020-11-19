using Instagrao.Domain.DTO;
using Instagrao.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Instagrao.Controllers
{
    [Route("api/[controller]")]
    public class InstagraoController : ControllerBase
    {
        /// <summary>
        /// Retorna os metadados das imagens
        /// </summary>
        /// <param name="s3ObjectKey"></param>
        /// <param name="getService"></param>
        /// <returns></returns>
        [HttpGet("{s3ObjectKey}")]
        public async Task<ActionResult<ImageMetadata>> GetMetadata(string s3ObjectKey, [FromServices] IGetService getService)
        {
            return await getService.Get(s3ObjectKey);
        }
    }
}
