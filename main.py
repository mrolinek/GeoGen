from cluster import save_metrics_params, update_params_from_cmdline

import os
import stat
import json

with open('settings.json') as f:
    orig_settings = json.load(f)

params = update_params_from_cmdline()
os.makedirs(params.working_dir, exist_ok=True)

input_path = os.path.join(params.input_dir, f'input_{params.folder_number}')
orig_settings["ProblemGeneratorInputProviderSettings"]["InputFolderPath"] = input_path
orig_settings["ProblemGenerationRunnerSettings"]["JsonOutputFolder"] = params.working_dir
orig_settings["Serilog"]["WriteTo"][1]["Args"]["Path"] = params.working_dir

st = os.stat('GeoGen.MainLauncher')
os.chmod('GeoGen.MainLauncher', st.st_mode | stat.S_IEXEC)

with open(os.path.join(params.working_dir, "new_settings.json"), "w") as f:
    json.dump(orig_settings, f)

os.system(f"./GeoGen.MainLauncher {os.path.join(params.working_dir, 'new_settings.json')}")

save_metrics_params({'done': 1}, params)
