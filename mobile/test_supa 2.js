const { createClient } = require('@supabase/supabase-js');
const SUPABASE_URL = 'https://mwlruauzporepqofkxbu.supabase.co';
const SUPABASE_KEY = 'sb_publishable_VSZ4ZReZMoLw44Q9bgzxFg_lsCNje7a';
const supabase = createClient(SUPABASE_URL, SUPABASE_KEY);

async function run() {
  const { data, error } = await supabase.from('spatial_entities').select('id, kind, geometry, confidence_state');
  console.log('Error:', error);
  console.log('Data sample:', data ? data.slice(0, 2) : null);
}
run();
