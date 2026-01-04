-- User-Defined Function: Calculate Task Priority Score
CREATE OR REPLACE FUNCTION fn_CalculateTaskPriority(p_dueDate TIMESTAMP WITH TIME ZONE, p_status TEXT)
RETURNS INT AS $$
DECLARE
    priority_score INT := 0;
    days_until_due INT;
BEGIN
    -- Completed tasks have zero priority
    IF p_status = 'Completed' THEN
        RETURN 0;
    END IF;
    
    -- Calculate days until due date
    days_until_due := EXTRACT(DAY FROM (p_dueDate - CURRENT_TIMESTAMP));
    
    -- Assign priority score based on urgency
    IF days_until_due < 0 THEN
        -- Overdue tasks get highest priority
        priority_score := 100;
    ELSIF days_until_due <= 1 THEN
        -- Due within 1 day
        priority_score := 80;
    ELSIF days_until_due <= 3 THEN
        -- Due within 3 days
        priority_score := 60;
    ELSIF days_until_due <= 7 THEN
        -- Due within 1 week
        priority_score := 40;
    ELSE
        -- More than a week away
        priority_score := 20;
    END IF;
    
    RETURN priority_score;
END;
$$ LANGUAGE plpgsql;