import { createClient } from '@supabase/supabase-js';

const SUPABASE_URL = 'https://example.supabase.co';
const SUPABASE_KEY = 'sb_publishable_REPLACE_ME';

export const supabase = createClient(SUPABASE_URL, SUPABASE_KEY, {
  auth: { persistSession: false },
});
