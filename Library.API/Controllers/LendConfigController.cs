﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Library.API.Configs.Filters;
using Library.API.Entities;
using Library.API.Helper;
using Library.Common.Models;
using Library.API.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LendConfigController : ControllerBase
    {
        #region field

        private readonly ILendConfigService _lendConfigService;
        private readonly IMapper _mapper;
        private readonly HashFactory _hashFactory;

        #endregion

        #region ctor

        public LendConfigController(IServicesWrapper repositoryWrapper, IMapper mapper, HashFactory hashFactory)
        {
            _lendConfigService = repositoryWrapper.LendConfig;
            _mapper = mapper;
            _hashFactory = hashFactory;
        }

        #endregion

        #region Get

        [HttpGet]
        public async Task<ActionResult<List<LendConfigDto>>> GetAllAsync()
        {
            var lendConfigs = await _lendConfigService.GetAllAsync();
            return lendConfigs.ProjectTo<LendConfigDto>(_mapper.ConfigurationProvider).ToList();
        }

        [HttpGet("{id:guid}", Name = nameof(GetAsync))]
        public async Task<ActionResult<LendConfigDto>> GetAsync(Guid id)
        {
            var lendConfig = await _lendConfigService.GetByIdAsync(id);
            var entityNewHash = _hashFactory.GetHash(lendConfig);
            Response.Headers[HeaderNames.ETag] = entityNewHash;
            return _mapper.Map<LendConfigDto>(lendConfig);
        }

        #endregion

        #region Post

        [HttpPost]
        [Authorize(Roles = "Administrator,SuperAdministrator")]
        public async Task<IActionResult> AddLendConfigAsync(LendConfigCreateDto dto)
        {
            if (dto.ReaderGrade is 0 or > 4)
            {
                throw new ArgumentException("Grade不合法，只能设置1,2,3,4");
            }

            var count = await _lendConfigService.CountByConditionAsync(config => config.ReaderGrade == dto.ReaderGrade);
            if (count != 0)
            {
                throw new ArgumentException("该Grade已经存在一个规则，请修改原有规则！");
            }

            var lendConfig = await _lendConfigService.AddAsync(_mapper.Map<LendConfig>(dto));
            if (lendConfig == null)
            {
                throw new Exception("创建资源LendConfig失败！");
            }

            var vo = _mapper.Map<LendConfigDto>(lendConfig);
            return CreatedAtRoute(nameof(GetAsync), new { id = lendConfig.Id }, vo);
        }

        #endregion

        #region Put

        [HttpPut("{id:guid}")]
        [CheckIfMatchHeaderFilter]
        public async Task<IActionResult> PutAsync(Guid id, LendConfigCreateDto dto)
        {
            var category = await _lendConfigService.GetByIdAsync(id);
            var entityHash = _hashFactory.GetHash(category);
            if (Request.Headers.TryGetValue(HeaderNames.IfMatch, out var requestETag) && requestETag != entityHash)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }

            _mapper.Map(dto, category);
            await _lendConfigService.UpdateAsync(category);
            var entityNewHash = _hashFactory.GetHash(category);
            Response.Headers[HeaderNames.ETag] = entityNewHash;
            return NoContent();
        }

        #endregion

        #region Delete

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var category = await _lendConfigService.GetByIdAsync(id);

            await _lendConfigService.DeleteAsync(category);
            return NoContent();
        }

        #endregion
    }
}