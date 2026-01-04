using AnalyticsService.Application.DTOs;
using AnalyticsService.Domain.Entities;
using AutoMapper;
using MongoDB.Bson;

namespace AnalyticsService.Application.Profiles;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskEvent, TaskEventDto>()
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata.ToDictionary(
                    x => x.Name,
                    x => BsonTypeMapper.MapToDotNetValue(x.Value))));

        CreateMap<TaskMetric, DashboardMetricsDto>()
            .ForMember(dest => dest.CompletionRate, opt => opt.MapFrom(src =>
                src.TotalTasks > 0 ? Math.Round((decimal)src.CompletedTasks / src.TotalTasks * 100, 2) : 0));
    }
}