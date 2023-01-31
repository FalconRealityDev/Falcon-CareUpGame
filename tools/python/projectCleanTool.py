import os.path
import pathlib
import platform
separator = "\\"

if platform.system() == "Linux":
	separator = "/"
	
	
BASE_DIR = os.path.dirname(os.path.realpath(__file__))
log_files = []
base_dir_fixed = ""

def build_used_file_list(list_path, format_index = 0):
	file1 = open(list_path, 'r')
	Lines = file1.readlines()
	count = 0
	for line in Lines:
		if len(line) > 0:
			if line[0] == " ":
				split_line = line.split(" ")
				file_path = ""
				for i in range(4, len(split_line)):
					file_path += split_line[i]
					if i < (len(split_line) - 1):
						file_path += " "
				if separator == "\\":
					file_path = file_path.replace("/", "\\")
				file_path = file_path.replace("\n", "")
				full_path = base_dir_fixed + file_path
				if os.path.isfile(full_path):
					#print(full_path)
					if not full_path in log_files:
						log_files.append(full_path)
		count += 1


base_dir_split = BASE_DIR.split(separator)

for i in range(len(base_dir_split)):
	if i < len(base_dir_split) - 2:
		base_dir_fixed += base_dir_split[i] + separator
base_dir_fixed += "care-up" + separator

build_used_file_list('careup_WebGL_build_log01.txt')
build_used_file_list('careup_Android_build_log01.txt')
build_used_file_list('organizer_files_log.txt')
print(BASE_DIR)
print(len(log_files))

dir_path = pathlib.Path(base_dir_fixed + "Assets" + separator)
counter = 0
files_to_remove = []


for item in list(dir_path.rglob("*")):
	if item.is_file():
		if not str(item).replace(".meta", "") in log_files:
			if ".meta" in str(item):
				continue
			if "Audio\Dialogue" in str(item):
				continue
			if "AddressableAssetsData" in str(item):
				continue
			if "editor" in str(item):
				continue
			if "SmartlookUnity" in str(item):
				continue
			if "MobileDependencyResolver" in str(item):
				continue
			if "ListOfActions" in str(item):
				continue
			if "WebGLTemplates\Better2020" in str(item):
				continue
			if "Plugins\WebGL" in str(item):
				continue
			if "IngameDebugConsole" in str(item):
				continue
			if "BuildTimestampDisplay" in str(item):
				continue
			if "CareUp_ActionEditor" in str(item):
				continue
			if "CareUp_AssetOrganizer" in str(item):
				continue
			if "CareUp_DictionaryEditor" in str(item):
				continue
			if "CareUp_ShapesToAnimation" in str(item):
				continue
			if "CustomShaders\Standard Two Sided Soft Blend.shader" in str(item):
				continue
			if "CustomShaders\TwoSided.shader" in str(item):
				continue
			if "Scenes_AEDSettings.lighting" in str(item):
				continue
			if "Scenes_Catherisation_WomenSettings.lighting" in str(item):
				continue
			if "Injection_Subcutaneous_In_HouseSettings.lighting" in str(item):
				continue
			if "Spatializer\Plugins" in str(item):
				continue
			print(str(item))
			files_to_remove.append(str(item))
			counter += 1
print(counter)


with open('files_to_delete.txt', "w", encoding="utf-8") as f:
	for p in files_to_remove:
		f.write(p + "\n")
