﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace CCServ.ClientAccess.DTOs.FAQEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class CreateOrUpdateFAQ : DTOBase
    {
        /// <summary>
        /// The faq the client wants to update or create.
        /// </summary>
        public Entities.FAQ FAQ { get; set; } 
        
        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public CreateOrUpdateFAQ(JObject obj) : base(obj)
        {
        }

        /// <summary>
        /// Simply returns the validation results for the FAQ entity.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Entities.FAQ.FAQValidator().Validate(this.FAQ);
        }
    }
}