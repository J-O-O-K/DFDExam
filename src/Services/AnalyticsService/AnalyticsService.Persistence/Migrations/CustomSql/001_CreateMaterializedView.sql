DROP MATERIALIZED VIEW IF EXISTS mv_daily_task_metrics CASCADE;

-- Materialized view for metrics
CREATE MATERIALIZED VIEW mv_daily_task_metrics AS
SELECT 
    ROW_NUMBER() OVER (ORDER BY date DESC) as id,
    date,
    total_tasks,
    completed_tasks,
    overdue_tasks,
    average_completion_time_hours,
    CASE 
        WHEN total_tasks > 0 
        THEN ROUND((completed_tasks::decimal / total_tasks::decimal) * 100, 2)
        ELSE 0.0 
    END as completion_rate
FROM task_metrics
ORDER BY date DESC;

CREATE UNIQUE INDEX idx_mv_daily_task_metrics_id ON mv_daily_task_metrics(id);
CREATE INDEX idx_mv_daily_task_metrics_date ON mv_daily_task_metrics(date DESC);

GRANT SELECT ON mv_daily_task_metrics TO postgres;