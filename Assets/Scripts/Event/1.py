r"""
将 Events 2.json（或其他事件 JSON）中的字段做批量替换：
- goldChange -> nobleChange
- zhouLiChange -> foreignChange
- weiwangChange -> scholarChange
并为每个 option 新增 kingChange（默认 0，若已存在则保留）
备份原文件为 .bak
在 Windows PowerShell / CMD 中运行：
python "d:/Work/unityChina/Game/DaZhouProject/zhihuProjecttttt-main/zhihuProjecttttt-main/Assets/Scripts/Event/1.py"
"""
import json
from pathlib import Path
import shutil

# 修改为你的目标文件路径
FILE = Path(r"d:\Work\unityChina\Game\DaZhouProject\zhihuProjecttttt-main\zhihuProjecttttt-main\Assets\Scripts\Event\Events.json")

def convert_option(opt: dict):
    # 新增 kingChange（若已存在则不覆盖）
    if "kingChange" not in opt:
        opt["kingChange"] = 0

    # 映射替换：优先保留已有目标字段
    if "goldChange" in opt and "nobleChange" not in opt:
        opt["nobleChange"] = opt.pop("goldChange")
    elif "goldChange" in opt:
        opt.pop("goldChange", None)

    if "zhouLiChange" in opt and "foreignChange" not in opt:
        opt["foreignChange"] = opt.pop("zhouLiChange")
    elif "zhouLiChange" in opt:
        opt.pop("zhouLiChange", None)

    if "weiwangChange" in opt and "scholarChange" not in opt:
        opt["scholarChange"] = opt.pop("weiwangChange")
    elif "weiwangChange" in opt:
        opt.pop("weiwangChange", None)

    # 如果原 JSON 已经部分使用了 nobleChange/scholarChange/foreignChange，保留现有值
    # 确保 peopleChange 存在（若没有则不强制新增，按需可改）
    if "peopleChange" not in opt:
        opt["peopleChange"] = 0

    return opt

def main():
    if not FILE.exists():
        print(f"文件不存在: {FILE}")
        return

    bak = FILE.with_suffix(FILE.suffix + ".bak")
    shutil.copy2(FILE, bak)
    print(f"已备份原文件到: {bak}")

    text = FILE.read_text(encoding="utf-8")
    # 直接解析 JSON（假设文件为有效 JSON）
    data = json.loads(text)

    if isinstance(data, list):
        for evt in data:
            if isinstance(evt, dict) and "options" in evt and isinstance(evt["options"], list):
                for i, opt in enumerate(evt["options"]):
                    if isinstance(opt, dict):
                        evt["options"][i] = convert_option(opt)
    else:
        print("文件不是 JSON 数组，未处理。")
        return

    FILE.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"转换完成并写回: {FILE}")

if __name__ == "__main__":
    main()