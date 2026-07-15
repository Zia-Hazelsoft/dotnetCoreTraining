using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;
using UserManagement.Api.Models;

namespace UserManagement.Api.Services
{
    /// <summary>
    /// Custom Sieve Processor defining filterable and sortable properties for the application.
    /// </summary>
    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(
            IOptions<SieveOptions> options) 
            : base(options)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            // Map User properties for filtering and sorting
            mapper.Property<User>(u => u.Id).CanFilter().CanSort();
            mapper.Property<User>(u => u.FirstName).CanFilter().CanSort();
            mapper.Property<User>(u => u.LastName).CanFilter().CanSort();
            mapper.Property<User>(u => u.Email).CanFilter().CanSort();
            mapper.Property<User>(u => u.EmailConfirmed).CanFilter().CanSort();

            return mapper;
        }
    }
}