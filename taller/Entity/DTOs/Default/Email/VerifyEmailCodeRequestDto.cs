﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs.Default.Email
{
    public class VerifyEmailCodeRequestDto
    {
        public string Code { get; set; } = null!;
    }

}
