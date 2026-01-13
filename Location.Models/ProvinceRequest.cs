using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Models
{

    #region Insert
    public class CreateProvinceRequest : IValidatableObject
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        public CreateProvince ToModel(string user) => new()
        {
            User = user,
            Name = this.Name,
            Code = this.Code
        };

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Name is required.", [nameof(Name)]);
            }
            if (string.IsNullOrWhiteSpace(Code))
            {
                yield return new ValidationResult("Code is required.", [nameof(Code)]);
            }
        }
    }

    public class CreateProvince : CreateProvinceRequest
    {
        public string User { get; set; } = string.Empty;
    }

    #endregion


    #region Update
    public class UpdateProvinceRequest : IValidatableObject
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int Id { get; set; }
        public UpdateProvince ToModel(string user) => new()
        {
            Id = this.Id,
            Name = this.Name,
            Code = this.Code,
            User = user
        };
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Id <= 0)
            {
                yield return new ValidationResult("Id must be greater than zero.", [nameof(Id)]);
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Name is required.", [nameof(Name)]);
            }
            if (string.IsNullOrWhiteSpace(Code))
            {
                yield return new ValidationResult("Code is required.", [nameof(Code)]);
            }
        }
    }
    public class UpdateProvince : UpdateProvinceRequest
    {
        public string User { get; set; } = string.Empty;
    }

    #endregion
}
