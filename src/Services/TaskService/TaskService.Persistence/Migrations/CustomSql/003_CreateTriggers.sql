-- Trigger Function: Audit Task Changes
CREATE OR REPLACE FUNCTION fn_TaskAudit()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'INSERT') THEN
        INSERT INTO task_audit_logs (task_id, action, changed_at, new_values)
        VALUES (NEW.id, 'Created', CURRENT_TIMESTAMP, row_to_json(NEW)::TEXT);
        RETURN NEW;
    ELSIF (TG_OP = 'UPDATE') THEN
        INSERT INTO task_audit_logs (task_id, action, changed_at, old_values, new_values)
        VALUES (NEW.id, 'Updated', CURRENT_TIMESTAMP, row_to_json(OLD)::TEXT, row_to_json(NEW)::TEXT);
        RETURN NEW;
    ELSIF (TG_OP = 'DELETE') THEN
        INSERT INTO task_audit_logs (task_id, action, changed_at, old_values)
        VALUES (OLD.id, 'Deleted', CURRENT_TIMESTAMP, row_to_json(OLD)::TEXT);
        RETURN OLD;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Drop trigger if exists
DROP TRIGGER IF EXISTS trg_TaskAudit ON tasks;

-- Create Trigger: Automatic Audit Logging
CREATE TRIGGER trg_TaskAudit
AFTER INSERT OR UPDATE OR DELETE ON tasks
FOR EACH ROW EXECUTE FUNCTION fn_TaskAudit();