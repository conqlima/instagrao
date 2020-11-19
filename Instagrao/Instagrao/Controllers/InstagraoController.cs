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

        /// <summary>
        /// Criar associação de questões similares
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("download/{s3ObjectKey}")]
        public string GetImage(string s3ObjectKey)
        {
            return "hello";
        }

        /// <summary>
        /// Criar associação de questões similares
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("info")]
        public string InfoImages()
        {
            return "hello";
        }
    }
}
