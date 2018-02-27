using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PgpCore;

namespace xpgp
{
    public class PgpController : Controller
    {
        [HttpGet]
        [Route("PGP/NewKeypair")]
        public IActionResult NewKeypair()
        {
            string publicKey;
            string privateKey;
            
            using (PGP pgp = new PGP())
            using (MemoryStream publicKeyStream = new MemoryStream())
            using (MemoryStream privateKeyStream = new MemoryStream())
            {
                pgp.GenerateKey(publicKeyStream, privateKeyStream);

                publicKey = Encoding.UTF8.GetString(publicKeyStream.ToArray());
                privateKey = Encoding.UTF8.GetString(privateKeyStream.ToArray());
            } 

            ViewBag.Text = publicKey + "\n\n" + privateKey;

            return View("Text");
        }
    }
}