from cluster import save_metrics_params, update_params_from_cmdline

import os
import stat
import json

with open('settings.json') as f:
    orig_settings = json.load(f)

params = update_params_from_cmdline()

input_path = os.path.join(params.input_dir, f'input_{params.folder_number}')
orig_settings["ProblemGeneratorInputProviderSettings"]["InputFolderPath"] = input_path
orig_settings["ProblemGenerationRunnerSettings"]["JsonBestTheoremFolder"] = params.model_dir
orig_settings["LoggingSettings"]["FileLogPath"] = params.model_dir

st = os.stat('GeoGen.MainLauncher')
os.chmod('GeoGen.MainLauncher', st.st_mode | stat.S_IEXEC)

os.system(f"./GeoGen.MainLauncher \'{json.dumps(orig_settings)}\'")

save_metrics_params({'done': 1}, params)
