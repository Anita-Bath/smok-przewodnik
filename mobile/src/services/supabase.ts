import { createClient } from '@supabase/supabase-js';

const SUPABASE_URL = 'https://mwlruauzporepqofkxbu.supabase.co';
const SUPABASE_KEY = 'sb_publishable_VSZ4ZReZMoLw44Q9bgzxFg_lsCNje7a';

export const supabase = createClient(SUPABASE_URL, SUPABASE_KEY, {
  auth: { persistSession: false },
});
