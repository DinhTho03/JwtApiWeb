using CreateUserequalBcrypt.Dto.User;
using CreateUserequalBcrypt.IService.Login;
using CreateUserequalBcrypt.IService.UserInfo;
using CreateUserequalBcrypt.Model.AccountModel;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CreateUserequalBcrypt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccount _user;
        public AuthController(IAccount user )
        {
            _user = user;
        }
        
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserInfoFirst userInfoFirst )
        {
            
            
            if (await _user.UserAlreadyExists(userInfoFirst))
            {
                return BadRequest("User already exists ,please try something else");
            }
            var user = _user.Register(userInfoFirst);
            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserDto userDto)
        {
            if (await _user.UserAlreadyExist(userDto))
            { 
                
                var login = _user.Login(userDto);
                if (login == null)
                {
                    return BadRequest("Username or password is incorrect");
                }
                return Ok(login);
            }
            return BadRequest("User does not exist");
        }

        [HttpPost("RequestToken")]
        public IActionResult Register (TokenModel token)
        {
            var refreshToken = _user.RenewToken(token);
            if (Login == null )
            {
                return BadRequest("data is null");
            }    
            return Ok(refreshToken);

        }
    }
}
