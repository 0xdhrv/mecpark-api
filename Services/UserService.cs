using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;

namespace WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string email, string password);
        void VerifyEmail(string token);
        void ForgotPassword(ForgotPasswordModel model, string orgin);
        void ResetPassword(ResetPasswordModel model);
        IEnumerable<User> GetUsers();
        IEnumerable<AllocationManager> GetAllocationManagers();
        IEnumerable<ParkingManager> GetParkingManagers();
        User GetUserById(int id);
        AllocationManager GetAllocationManagerById(int id);
        ParkingManager GetParkingManagerById(int id);
        User Create(User user, string password, string origin);
        void Update(User user, string password = null);
        void Delete(int id);
        string getRole(int id);
        int GetAllocationManagerId(int id);
        int GetParkingManagerId(int id);
    }

    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IEmailService _emailService;

        public UserService(DataContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public User Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(x => x.Email == email);

            // Checking if Email Exists
            if (user == null)
                return null;

            // Checking if Password is Correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // Authentication Successful and User return
            return user;
        }

        public IEnumerable<User> GetUsers()
        {
            // Return All Users
            return _context.Users;
        }

        public IEnumerable<AllocationManager> GetAllocationManagers()
        {
            // Return All AllocationManagers
            return _context.AllocationManagers;
        }

        public IEnumerable<ParkingManager> GetParkingManagers()
        {
            // Return All ParkingManagers
            return _context.ParkingManagers;
        }

        public User GetUserById(int id)
        {
            // Return User By Id
            return _context.Users.Find(id);
        }
        public AllocationManager GetAllocationManagerById(int id)
        {
            // Return AllocationManager by Id
            return _context.AllocationManagers.Find(id);
        }

        public ParkingManager GetParkingManagerById(int id)
        {
            // Return ParkingManager by Id 
            return _context.ParkingManagers.Find(id);
        }

        public User Create(User user, string password, string origin)
        {
            AllocationManager allocationManager = new AllocationManager();
            ParkingManager parkingManager = new ParkingManager();
            
            // No Password Provided
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            // Checking If Email ALready Exists
            if (_context.Users.Any(x => x.Email == user.Email))
                throw new AppException("Email " + user.Email + " is already taken");

            // Checking If the User is First User
            var isFirstUser = _context.Users.Count() == 0;

            // if First User, then Admin Role is granted
            if (isFirstUser)
            {
                user.Role = "Admin";
            }

            // various conditionals to revoke another Admin Role
            // and, pass relevant User role to users
            if (string.IsNullOrWhiteSpace(user.Role))
            {
                user.Role = "User";
            }

            if (user.Role != "User" && user.Role != "AllocationManager" && user.Role != "ParkingManager" && user.Role != "Admin")
            {
                user.Role = "User";
            }

            var role = user.Role == "Admin";

            if (user.Role == "Admin" && !isFirstUser)
            {
                user.Role = "User";
            }

            // declaring PasswordHash and PasswordSalt Blobs
            byte[] passwordHash, passwordSalt;

            // Creating PasswordHash and PasswordSalt for Password
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // Saving Data for AllocationManager
            if (user.Role == "AllocationManager")
            {
                allocationManager.Name = user.Name;
                allocationManager.Email = user.Email;
                allocationManager.Address = user.Address;
                allocationManager.City = user.City;
                allocationManager.State = user.State;
                allocationManager.Phone = user.Phone;
                allocationManager.PasswordHash = user.PasswordHash;
                allocationManager.PasswordSalt = user.PasswordSalt;
                allocationManager.Created = DateTime.Now;
                _context.AllocationManagers.Add(allocationManager);
                _context.SaveChanges();
            }

            // Saving Data for ParkingManager
            if (user.Role == "ParkingManager")
            {
                parkingManager.Name = user.Name;
                parkingManager.Email = user.Email;
                parkingManager.Address = user.Address;
                parkingManager.City = user.City;
                parkingManager.State = user.State;
                parkingManager.Phone = user.Phone;
                parkingManager.PasswordHash = user.PasswordHash;
                parkingManager.PasswordSalt = user.PasswordSalt;
                parkingManager.Created = DateTime.Now;
                _context.ParkingManagers.Add(parkingManager);
                _context.SaveChanges();
            }

            user.Created = DateTime.Now;
            user.VerificationToken = randomTokenString();
            _context.Users.Add(user);
            _context.SaveChanges();

            // Comment Out to send Verification Email
            // sendVerificationEmail(user, origin);

            return user;
        }

        public void VerifyEmail(string token)
        {
            var user = _context.Users.SingleOrDefault(x => x.VerificationToken == token);

            // Checking if User exists with Same Verification Token
            if (user == null) throw new AppException("Verification failed");

            // Updating AllocationManager too, if exists
            if (user.Role == "AllocationManager")
            {
                var allocationManager = _context.AllocationManagers.Single(a => a.Email == user.Email);
                allocationManager.Verified = DateTime.Now;
                _context.AllocationManagers.Update(allocationManager);
                _context.SaveChanges();
            }

            // Updating ParkingManager too, if exists
            if (user.Role == "ParkingManager")
            {
                var parkingManager = _context.ParkingManagers.Single(a => a.Email == user.Email);
                parkingManager.Verified = DateTime.Now;
                _context.ParkingManagers.Update(parkingManager);
                _context.SaveChanges();
            }

            user.Verified = DateTime.Now;
            user.VerificationToken = null;

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void ForgotPassword(ForgotPasswordModel model, string origin)
        {
            // Checking if User exists
            var user = _context.Users.SingleOrDefault(x => x.Email == model.Email);
            // not sending anything to avoid that information
            if (user == null) return;

            // generating a random ResetToken
            user.ResetToken = randomTokenString();
            // ResetToken expiration set to 1 hour after Generation
            user.ResetTokenExpires = DateTime.Now.AddHours(1);

            _context.Users.Update(user);
            _context.SaveChanges();

            // sending Password Reset Email
            sendPasswordResetEmail(user, origin);
        }

        public void ResetPassword(ResetPasswordModel model)
        {
            // Checking if User exists with an unexpired ResetToken
            var user = _context.Users.SingleOrDefault(x =>
                x.ResetToken == model.Token &&
                x.ResetTokenExpires > DateTime.Now);

            if (user == null)
                throw new AppException("Invalid Token");

            // Resetting Password
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);

            // Updating AllocationManager
            if (user.Role == "AllocationManager")
            {
                var allocationManager = _context.AllocationManagers.Single(a => a.Email == user.Email);
                allocationManager.PasswordHash = passwordHash;
                allocationManager.PasswordSalt = passwordSalt;
                allocationManager.PasswordReset = DateTime.Now;
                _context.AllocationManagers.Update(allocationManager);
                _context.SaveChanges();
            }

            // Updating ParkingManager
            if (user.Role == "ParkingManager")
            {
                var parkingManager = _context.ParkingManagers.Single(a => a.Email == user.Email);
                parkingManager.PasswordHash = passwordHash;
                parkingManager.PasswordSalt = passwordSalt;
                parkingManager.PasswordReset = DateTime.Now;
                _context.ParkingManagers.Update(parkingManager);
                _context.SaveChanges();
            }

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordReset = DateTime.Now;
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Update(User userParam, string password = null)
        {
            var tempUser = _context.Users.Find(userParam.Id);
            var role = tempUser.Role;
            var id = tempUser.Id;
            var email = tempUser.Email;
            userParam.Role = role;


            if (userParam.Role == "AllocationManager")
            {
                var user = _context.Users.Find(userParam.Id);
                var allocationManager = _context.AllocationManagers.Single(a => a.Email == userParam.Email);

                if (user == null || allocationManager == null)
                    throw new AppException("AllocationManager not found");

                // Update Data if provided and is AllocationManager
                if (!string.IsNullOrWhiteSpace(userParam.Name))
                {
                    user.Name = allocationManager.Name = userParam.Name;
                }


                if (!string.IsNullOrWhiteSpace(userParam.Email) && userParam.Email != user.Email)
                {
                    // Check if Email is Already Registered
                    if (_context.Users.Any(x => x.Email == userParam.Email) || _context.AllocationManagers.Any(x => x.Email == userParam.Email))
                        throw new AppException("Email " + userParam.Email + " is already taken");

                    user.Email = allocationManager.Email = userParam.Email;
                }

                if (!string.IsNullOrWhiteSpace(userParam.Address))
                    user.Address = allocationManager.Address = userParam.Address;

                if (!string.IsNullOrWhiteSpace(userParam.City))
                    user.City = allocationManager.City = userParam.City;

                if (!string.IsNullOrWhiteSpace(userParam.State))
                    user.State = allocationManager.City = userParam.State;

                // Update User Password
                if (!string.IsNullOrWhiteSpace(password))
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    user.PasswordHash = allocationManager.PasswordHash = passwordHash;
                    user.PasswordSalt = allocationManager.PasswordSalt = passwordSalt;
                }

                user.Updated = DateTime.Now;
                allocationManager.Updated = DateTime.Now;
                _context.Users.Update(user);
                _context.AllocationManagers.Update(allocationManager);
                _context.SaveChanges();
            }

            // Update Data if provided and is ParkingManager
            if (userParam.Role == "ParkingManager")
            {
                var user = _context.Users.Find(userParam.Id);
                var parkingManager = _context.ParkingManagers.Single(p => p.Email == userParam.Email);
                if (user == null || parkingManager == null)
                    throw new AppException("ParkingManager not found");

                if (!string.IsNullOrWhiteSpace(userParam.Name))
                {
                    user.Name = parkingManager.Name = userParam.Name;
                }


                // Check if Email is Already Registered
                if (!string.IsNullOrWhiteSpace(userParam.Email) && userParam.Email != user.Email)
                {
                    // throw error if the new email is already taken
                    if (_context.Users.Any(x => x.Email == userParam.Email) || _context.ParkingManagers.Any(x => x.Email == userParam.Email))
                        throw new AppException("Email " + userParam.Email + " is already taken");

                    user.Email = parkingManager.Email = userParam.Email;
                }

                // update user properties if provided

                if (!string.IsNullOrWhiteSpace(userParam.Address))
                    user.Address = parkingManager.Address = userParam.Address;

                if (!string.IsNullOrWhiteSpace(userParam.City))
                    user.City = parkingManager.City = userParam.City;

                if (!string.IsNullOrWhiteSpace(userParam.State))
                    user.State = parkingManager.City = userParam.State;

                // update password if provided
                if (!string.IsNullOrWhiteSpace(password))
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    user.PasswordHash = parkingManager.PasswordHash = passwordHash;
                    user.PasswordSalt = parkingManager.PasswordSalt = passwordSalt;
                }

                user.Updated = DateTime.Now;
                parkingManager.Updated = DateTime.Now;
                _context.Users.Update(user);
                _context.ParkingManagers.Update(parkingManager);
                _context.SaveChanges();
            }

            if (userParam.Role == "User")
            {
                var user = _context.Users.Find(userParam.Id);
                if (user == null)
                    throw new AppException("User not found");

                if (!string.IsNullOrWhiteSpace(userParam.Name))
                {
                    user.Name = userParam.Name;
                }


                if (!string.IsNullOrWhiteSpace(userParam.Email) && userParam.Email != user.Email)
                {
                    if (_context.Users.Any(x => x.Email == userParam.Email))
                        throw new AppException("Email " + userParam.Email + " is already taken");

                    user.Email = userParam.Email;
                }

                if (!string.IsNullOrWhiteSpace(userParam.Address))
                    user.Address = userParam.Address;

                if (!string.IsNullOrWhiteSpace(userParam.City))
                    user.City = userParam.City;

                if (!string.IsNullOrWhiteSpace(userParam.State))
                    user.State = userParam.State;

                if (!string.IsNullOrWhiteSpace(password))
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }

                user.Updated = DateTime.Now;
                _context.Users.Update(user);
                _context.SaveChanges();
            }


        }

        public void Delete(int id)
        {
            var user = _context.Users.Find(id);

            if (user != null)
            {
                if (user.Role == "AllocationManager")
                {
                    var allocationManager = _context.AllocationManagers.Single(a => a.Email == user.Email);
                    _context.AllocationManagers.Remove(allocationManager);
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                }

                if (user.Role == "ParkingManager")
                {
                    var parkingManager = _context.ParkingManagers.Single(p => p.Email == user.Email);
                    _context.ParkingManagers.Remove(parkingManager);
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                }

                if (user.Role == "User")
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                }

            }
        }

        public string getRole(int id)
        {
            var user = _context.Users.Find(id);
            return user.Role;
        }

        public int GetAllocationManagerId(int id)
        {
            var user = _context.Users.Find(id);
            var allocationManager = _context.AllocationManagers.SingleOrDefault(a => a.Email == user.Email);
            if (allocationManager == null)
                return 0;
            return (allocationManager.Id);
        }
        public int GetParkingManagerId(int id)
        {
            var user = _context.Users.Find(id);
            var parkingManager = _context.ParkingManagers.SingleOrDefault(p => p.Email == user.Email);
            if (parkingManager == null)
                return 0;
            return (parkingManager.Id);
        }


        private string randomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private void sendVerificationEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var verifyUrl = $"{origin}/users/verify-email?token={user.VerificationToken}";
                message = $@"<p>Please Click the Below Link to Verify Your Email Address:</p>
                             <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the Below Token to Verify Your Email Address with the <code>/users/verify-email</code></p>
                             <p><code>{user.VerificationToken}</code></p>";
            }

            _emailService.Send(
                to: user.Email,
                subject: "MecPark - Verification Email",
                html: $@"<h4>MecPark | Verify Email</h4>
                         <p>Thanks for Registering</p>
                         {message}"
            );
        }

        private void sendPasswordResetEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/users/reset-password?token={user.ResetToken}";
                message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 hour:</p>
                             <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password with the <code>/users/reset-password</code></p>
                             <p><code>{user.ResetToken}</code></p>";
            }

            _emailService.Send(
                to: user.Email,
                subject: "MecPark - Password Reset Email",
                html: $@"<h4>MecPark | Reset Password</h4>
                         {message}"
            );
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password Required", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid Hash Length", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid Salt Length", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}