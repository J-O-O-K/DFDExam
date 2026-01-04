-- Stored Procedure: Get High Priority Tasks
CREATE OR REPLACE FUNCTION sp_GetHighPriorityTasks()
RETURNS TABLE(
    Id INT,
    Title VARCHAR(200),
    Description VARCHAR(2000),
    Status TEXT,
    Priority TEXT,
    DueDate TIMESTAMP WITH TIME ZONE,
    AssignedTo VARCHAR(100),
    CalculatedPriority INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        t.id,
        t.title,
        t.description,
        t.status,
        t.priority,
        t.due_date,
        t.assigned_to,
        fn_CalculateTaskPriority(t.due_date, t.status) as CalculatedPriority
    FROM tasks t
    WHERE t.priority = 'High' OR fn_CalculateTaskPriority(t.due_date, t.status) > 60
    ORDER BY CalculatedPriority DESC;
END;
$$ LANGUAGE plpgsql;

-- Stored Procedure: Get Overdue Tasks
CREATE OR REPLACE FUNCTION sp_GetOverdueTasks()
RETURNS TABLE(
    Id INT,
    Title VARCHAR(200),
    Description VARCHAR(2000),
    Status TEXT,
    Priority TEXT,
    DueDate TIMESTAMP WITH TIME ZONE,
    AssignedTo VARCHAR(100),
    DaysOverdue INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        t.id,
        t.title,
        t.description,
        t.status,
        t.priority,
        t.due_date,
        t.assigned_to,
        EXTRACT(DAY FROM (CURRENT_TIMESTAMP - t.due_date))::INT as DaysOverdue
    FROM tasks t
    WHERE t.due_date < CURRENT_TIMESTAMP AND t.status != 'Completed'
    ORDER BY t.due_date ASC;
END;
$$ LANGUAGE plpgsql;