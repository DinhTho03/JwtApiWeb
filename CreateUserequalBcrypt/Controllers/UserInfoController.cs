using CreateUserequalBcrypt.IService.Login;
using CreateUserequalBcrypt.IService.UserInfo;
using CreateUserequalBcrypt.Model.UserInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CreateUserequalBcrypt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly IUserInfoResposity _userInfo;
        private readonly IAccount _account;

        public UserInfoController(IUserInfoResposity userInfo ,IAccount account)
        {
            _userInfo = userInfo;
            _account = account;
        }
        // GET: api/<UserInfoController>

        [HttpGet, Authorize]
        public IActionResult Get()
        {
            
            return Ok(_userInfo.GetAll());
        }

        // GET api/<UserInfoController>/5
        [HttpGet("{id}"), Authorize]
        public async Task<IActionResult> Get(int id)
        {
            if (await _userInfo.IdAlreadyExist(id))
            {
                var data = _userInfo.GetById(id);
                return Ok(data);
            }
            return NotFound();
        }

        // POST api/<UserInfoController>
        [HttpPost , Authorize (Roles = "True")]
        public IActionResult Add(AddUserInfo userInfo)
        {
            
            if (userInfo != null)
            {
                return Ok(_userInfo.Add(userInfo));
            }
            return NotFound();

        }

        // PUT api/<UserInfoController>/5
        [HttpPut("{id}"), Authorize(Roles = "True")]
        public async Task<IActionResult> UpdateByID(int id, UpdateModel update)
        {

            if (await _userInfo.IdAlreadyExist(id))
            {
                _userInfo.Update(id, update);
                return Ok();
            }
            return NoContent();

        }

        // DELETE api/<UserInfoController>/5
        [HttpDelete("{id}"), Authorize(Roles = "True")]
        public async Task<IActionResult> DeleteUser(int id)
        {

            if (await _userInfo.IdAlreadyExist(id))
            {
                _userInfo.Delete(id);
                return Ok();
            }
            return NoContent();
        }
    }
}
