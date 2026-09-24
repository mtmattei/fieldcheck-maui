#!/usr/bin/env python3
"""Verification harness: drives the FieldCheck Release APK on an emulator via adb + uiautomator.
Usage: driver.py <apk> <outDir> <inspection-photo.png>. Writes screenshots and android-checks.json."""
import json, os, re, subprocess, sys, time, traceback
import xml.etree.ElementTree as ET

APK, OUT, PHOTO = sys.argv[1], os.path.abspath(sys.argv[2]), os.path.abspath(sys.argv[3])
PKG = "com.fieldcheck.app"
SHOTS = os.path.join(OUT, "screenshots")
os.makedirs(SHOTS, exist_ok=True)
checks = []


def adb(*args, check=False, timeout=120):
    r = subprocess.run(["adb", *args], capture_output=True, text=True, timeout=timeout)
    if check and r.returncode:
        raise RuntimeError(f"adb {' '.join(args)} failed: {r.stderr}")
    return r.stdout


def sh(cmd):
    return adb("shell", cmd)


def record(cid, ok, detail):
    checks.append({"id": cid, "status": "PASS" if ok else "FAIL", "detail": detail})
    print(f"   [{'PASS' if ok else 'FAIL'}] {cid}: {detail}", flush=True)


class Node:
    def __init__(self, a):
        self.text = a.get("text", "")
        self.desc = a.get("content-desc", "")
        self.rid = a.get("resource-id", "")
        self.cls = a.get("class", "")
        self.checked = a.get("checked") == "true"
        self.enabled = a.get("enabled") == "true"
        m = re.findall(r"\d+", a.get("bounds", "[0,0][0,0]"))
        self.x1, self.y1, self.x2, self.y2 = map(int, m)

    @property
    def label(self):
        return self.text or self.desc

    def center(self):
        return (self.x1 + self.x2) // 2, (self.y1 + self.y2) // 2

    def visible(self):
        return self.x2 > self.x1 and self.y2 > self.y1 and self.y1 >= 0 and self.y2 <= H - 10

    def __repr__(self):
        return f"<{self.cls.split('.')[-1]} text={self.text!r} desc={self.desc!r} [{self.x1},{self.y1}][{self.x2},{self.y2}]>"


def dump():
    for _ in range(3):
        sh("uiautomator dump /sdcard/ui.xml >/dev/null 2>&1")
        raw = adb("exec-out", "cat", "/sdcard/ui.xml")
        try:
            return [Node(n.attrib) for n in ET.fromstring(raw).iter("node")]
        except ET.ParseError:
            time.sleep(1)
    return []


def wait(pred, timeout=20):
    end = time.time() + timeout
    while time.time() < end:
        for n in dump():
            if pred(n):
                return n
        time.sleep(0.7)
    return None


def find_all(pred):
    return [n for n in dump() if pred(n)]


def by_id(i):
    return lambda n: n.rid.endswith(":id/" + i) or n.rid == i or n.desc == i


def text_is(t):
    return lambda n: n.text == t or n.desc == t


def text_ci(t):
    return lambda n: n.text.lower() == t.lower()


def desc_starts(t):
    return lambda n: n.desc.startswith(t) or n.text.startswith(t)


def tap(n):
    x, y = n.center()
    sh(f"input tap {x} {y}")
    time.sleep(0.8)


def swipe_up():
    sh(f"input swipe {W // 2} {int(H * 0.72)} {W // 2} {int(H * 0.32)} 400")
    time.sleep(0.8)


def swipe_down():
    sh(f"input swipe {W // 2} {int(H * 0.3)} {W // 2} {int(H * 0.8)} 400")
    time.sleep(0.8)


def find_scroll(pred, swipes=6, timeout=6):
    n = wait(pred, timeout)
    for _ in range(swipes):
        if n and n.visible() and n.y2 < H - 60:
            return n
        swipe_up()
        n = wait(pred, 2)
    return n if n and n.visible() else None


def tap_scroll(pred, what):
    n = find_scroll(pred)
    if not n:
        raise RuntimeError(f"not found: {what}")
    tap(n)
    return n


def hide_keyboard():
    if "mInputShown=true" in sh("dumpsys input_method"):
        sh("input keyevent 4")
        time.sleep(0.8)


def type_text(t):
    sh("input text " + t.replace(" ", "%s"))
    time.sleep(0.6)


def clear_field():
    sh("input keyevent 123")  # MOVE_END
    sh("input keyevent " + " ".join(["67"] * 30))
    time.sleep(0.4)


def shot(name):
    path = os.path.join(SHOTS, name + ".png")
    with open(path, "wb") as f:
        f.write(subprocess.run(["adb", "exec-out", "screencap", "-p"], capture_output=True).stdout)
    try:
        from PIL import Image
        img = Image.open(path)
        img.resize((412, 915), Image.LANCZOS).save(os.path.join(SHOTS, name + "-412x915.png"))
    except Exception as ex:
        print("   (downscale skipped:", ex, ")")


def bottom_tab(name):
    tabs = [n for n in dump() if n.label == name and n.y1 > H * 0.85]
    if not tabs:
        raise RuntimeError(f"bottom tab {name} not found")
    tap(max(tabs, key=lambda n: n.y1))


ACTIVITY = f"{PKG}/.MainActivity"


def dismiss_system_dialogs():
    for _ in range(3):
        nodes = dump()
        if not any("isn't responding" in n.text or "keeps stopping" in n.text for n in nodes):
            return
        btn = next((n for n in nodes if n.text in ("Wait", "Close app", "OK")), None)
        if btn:
            tap(btn)
        time.sleep(1)


def launch():
    dismiss_system_dialogs()
    out = sh(f"am start -W -n {ACTIVITY}")
    print("   am start:", out.strip().replace("\n", " | "), flush=True)
    m = re.search(r"TotalTime: (\d+)", out)
    time.sleep(2)
    dismiss_system_dialogs()
    return int(m.group(1)) if m else None, ACTIVITY


def rows():
    return [n.desc for n in dump() if ", status " in n.desc]


def step(name, fn):
    print("==", name, flush=True)
    try:
        fn()
    except Exception as ex:
        traceback.print_exc()
        record("step:" + name, False, str(ex))
        try:
            shot("fail-" + name)
        except Exception:
            pass


# ---------------------------------------------------------------- setup
sh("settings put system font_scale 1.0")
size = re.findall(r"(\d+)x(\d+)", sh("wm size"))[-1]
W, H = int(size[0]), int(size[1])
density = sh("wm density").strip()
print("screen", W, H, density)
t0 = time.time()
print(adb("install", "-r", APK, timeout=300))
install_s = time.time() - t0
adb("push", PHOTO, "/sdcard/Download/inspection-photo.png")
sh("am broadcast -a android.intent.action.MEDIA_SCANNER_SCAN_FILE -d file:///sdcard/Download/inspection-photo.png")
sh("content call --uri content://media --method scan_volume --arg external_primary")
sh("logcat -c")
time.sleep(15)  # let the launcher settle after boot
sh("input keyevent 3")
time.sleep(2)
dismiss_system_dialogs()
state = {}


def s_launch():
    ms, activity = launch()
    state["activity"] = activity
    greet = wait(lambda n: re.match(r"Good (morning|afternoon|evening)$", n.text), 60)
    record("A05", greet is not None, f"Dashboard '{greet and greet.text}' shown; am start TotalTime={ms} ms; install {install_s:.1f}s; screen {W}x{H} density {density}")
    record("J04", greet is not None, "Clean install first run seeded data and reached Dashboard")
    time.sleep(1.5)
    shot("01-dashboard")
    labels = [n.text for n in dump()]
    record("C02-ui", all(x in labels for x in ["12", "7", "3", "2"]), "Dashboard counts 12/7/3/2 visible")
    record("F01-dashboard", wait(text_is("View all assets"), 3) is not None or find_scroll(text_is("View all assets")) is not None, "Dashboard content reachable at 412x915-class viewport")


def s_assets():
    bottom_tab("Assets")
    wait(text_is("Assets"), 20)
    time.sleep(1)
    record("B07", wait(lambda n: n.text.endswith("equipment records"), 10) is not None, "Bottom navigation reached Assets")
    shot("02-assets")
    search = wait(lambda n: n.cls.endswith("EditText"), 10)
    tap(search)
    type_text("roof")
    hide_keyboard()
    time.sleep(1)
    ids = sorted(d.split(",")[1].strip() for d in rows())
    record("C03-ui", ids == ["AHU-203", "CT-007", "FAN-305"], "Search 'roof' -> " + ",".join(ids))
    tap(wait(desc_starts("Filter: Critical")))
    time.sleep(1)
    ids = [d.split(",")[1].strip() for d in rows()]
    record("C05-ui", ids == ["CT-007"], "roof + Critical -> " + ",".join(ids))
    tap(wait(lambda n: n.cls.endswith("EditText")))
    clear_field()
    type_text("zzz")
    hide_keyboard()
    nores = wait(text_is("No matching assets"), 10)
    shot("e05-no-results")
    record("E05-ui", nores is not None, "No-results state for 'zzz'")
    tap(wait(text_is("Clear search and filter")))
    time.sleep(1)
    record("C04-ui", len(rows()) >= 5, f"Cleared: {len(rows())} rows visible")


def s_detail():
    tap_scroll(desc_starts("Cooling Tower 07,"), "CT-007 row")
    title = wait(text_is("Asset detail"), 15)
    time.sleep(1.2)
    record("B03", title is not None and wait(text_is("CT-007"), 5) is not None, "Asset Detail opened for CT-007")
    shot("03-asset-detail")
    swipe_up()
    shot("e07-asset-detail-scrolled")
    btn = find_scroll(text_is("Start inspection"))
    record("E07", btn is not None and wait(lambda n: "intentionally long description" in n.text, 3) is not None, "Long description readable; Start inspection reachable")


def s_back():
    tap(find_scroll(text_is("Start inspection")))
    wait(text_is("New inspection"), 15)
    time.sleep(1)
    hide_keyboard()
    sh("input keyevent 4")
    back1 = wait(text_is("Asset detail"), 10)
    sh("input keyevent 4")
    back2 = wait(lambda n: n.text.endswith("equipment records"), 10)
    record("B08", back1 is not None and back2 is not None, "System back: Inspection -> Asset Detail -> Assets")
    record("F06", back2 is not None and PKG in sh("dumpsys window | grep mCurrentFocus"), "Back did not exit the app while in-app destinations existed")


def s_form():
    tap_scroll(desc_starts("Cooling Tower 07,"), "CT-007 row")
    wait(text_is("Asset detail"), 15)
    tap(find_scroll(text_is("Start inspection")))
    wait(text_is("New inspection"), 15)
    time.sleep(1.2)
    record("B04", wait(text_is("Cooling Tower 07 · CT-007"), 5) is not None, "New inspection shows asset identity")
    shot("h04-new-inspection-empty")
    submit = find_scroll(text_is("Submit inspection"))
    record("D14-ui", submit is not None and not submit.enabled, "Submit disabled on empty form")
    swipe_down(); swipe_down()
    tap(wait(desc_starts("Condition Good")))
    time.sleep(0.8)
    record("D10-ui", wait(text_ci("Issue description"), 2) is None, "Good + Yes: issue description hidden")
    tap(wait(desc_starts("Operating normally")))
    record("D12-ui", wait(text_ci("Issue description"), 5) is not None, "Operating No: issue description shown")
    tap(wait(desc_starts("Operating normally")))
    tap(wait(desc_starts("Condition Attention")))
    record("D11-ui", wait(text_ci("Issue description"), 5) is not None, "Attention: issue description shown")
    temp = sorted([n for n in dump() if n.cls.endswith("EditText")], key=lambda n: n.y1)[0]
    tap(temp)
    type_text("300")
    hide_keyboard()
    err = wait(lambda n: "between -50 and 250" in n.text, 5)
    shot("d04-temperature-validation")
    record("D04-ui", err is not None, "300 °C shows range error")
    tap(sorted([n for n in dump() if n.cls.endswith("EditText")], key=lambda n: n.y1)[0])
    clear_field()
    type_text("27")
    hide_keyboard()
    record("D03-ui", wait(lambda n: "between -50 and 250" in n.text, 2) is None, "27 °C accepted")
    for cb in sorted([n for n in dump() if n.cls.endswith("CheckBox")], key=lambda n: n.y1)[:3]:
        tap(cb)
    boxes = [n for n in dump() if n.cls.endswith("CheckBox")]
    record("D05-ui", len(boxes) == 3 and all(b.checked for b in boxes), "Three checklist items checked")
    issue = find_scroll(lambda n: n.cls.endswith("EditText") and ("Describe" in n.text or "Issue" in n.desc))
    tap(issue)
    type_text("Basin level alarm intermittent")
    hide_keyboard()
    notes = find_scroll(lambda n: n.cls.endswith("EditText") and ("Optional" in n.text or "Notes" in n.desc))
    tap(notes)
    type_text("Line one")
    sh("input keyevent 66")
    type_text("Line two")
    hide_keyboard()
    record("F02", find_scroll(text_is("Submit inspection")) is not None, "Form scrolls; submit reachable after keyboard dismissal")
    # background / resume keeps state
    sh("input keyevent 3")
    time.sleep(2)
    sh(f"am start -n {state['activity']}")
    time.sleep(2)
    swipe_down(); swipe_down(); swipe_down()
    record("J06", wait(lambda n: n.cls.endswith("EditText") and n.text == "27", 8) is not None, "Background/resume kept form input (temperature 27)")


def s_picker():
    tap_scroll(desc_starts("Attach photo or file"), "attach")
    picker = wait(lambda n: "documentsui" in n.rid or n.text in ("Recent", "Downloads", "Images") or n.desc == "Show roots", 20)
    time.sleep(1.5)
    shot("d07-picker-open")
    record("D07", picker is not None, f"System document picker opened ({picker})")
    sh("input keyevent 4")
    time.sleep(2)
    back = wait(text_is("New inspection"), 10)
    record("D09", back is not None and wait(lambda n: "Attached" == n.text, 2) is None, "Picker cancel left the form usable with no attachment")
    tap_scroll(desc_starts("Attach photo or file"), "attach")
    wait(lambda n: "documentsui" in n.rid or n.desc == "Show roots", 20)
    time.sleep(1.5)
    f = wait(lambda n: "inspection-photo" in n.label, 4)
    if not f:
        roots = wait(lambda n: n.desc == "Show roots", 5)
        if roots:
            tap(roots)
        tap(wait(text_is("Downloads"), 10))
        time.sleep(1.5)
        f = wait(lambda n: "inspection-photo" in n.label, 10)
    shot("d08-picker-file")
    tap(f)
    name = wait(text_is("inspection-photo.png"), 20)
    time.sleep(1)
    record("D08", name is not None, "Selected file name shown in form")
    record("F08", name is not None, "Android Storage Access Framework picker returned the pushed test image")
    swipe_down(); swipe_down(); swipe_down(); swipe_down()
    shot("04-new-inspection")
    swipe_up()
    shot("04-new-inspection-lower")


def s_submit():
    btn = find_scroll(text_is("Submit inspection"))
    record("D14-ui-enabled", btn is not None and btn.enabled, "Submit enabled when valid")
    x, y = btn.center()
    sh(f"input tap {x} {y} & input tap {x} {y} & input tap {x} {y}")
    ident = wait(lambda n: n.text.startswith("INS-"), 20)
    time.sleep(1.2)
    shot("05-inspection-success")
    record("D17-ui", ident is not None and ident.text == "INS-24092" and wait(text_is("Cooling Tower 07"), 3) is not None, f"Success shows {ident and ident.text}")
    tap(wait(text_is("View asset")))
    detail = wait(text_is("Asset detail"), 15)
    time.sleep(1)
    today = time.strftime("%b ") + str(int(time.strftime("%d"))) + time.strftime(", %Y")
    ok = detail is not None and wait(text_is(today), 5) is not None and wait(text_is("Attention condition"), 5) is not None
    shot("d18-asset-after-save")
    record("D18-ui", ok, f"Asset detail shows last inspection {today}, Attention condition")
    record("B12-view-asset", detail is not None, "View asset returned to Asset Detail")


def s_history():
    sh("input keyevent 4")
    wait(lambda n: n.text.endswith("equipment records"), 10)
    bottom_tab("History")
    wait(lambda n: n.text.endswith("completed inspections"), 15)
    time.sleep(1.2)
    shot("06-history")
    ids = [n.text for n in sorted(dump(), key=lambda n: n.y1) if re.match(r"INS-\d+ · ", n.text)]
    record("B07-history", bool(ids), "Bottom navigation reached History")
    record("C06-ui", bool(ids) and ids[0].startswith("INS-24092 · Today"), "Order: " + " | ".join(ids))
    record("D15-ui", sum(i.startswith("INS-24092") for i in ids) == 1 and not any(i.startswith("INS-24093") for i in ids), "Triple tap created exactly one inspection")
    record("C09-ui", wait(text_is("7 completed inspections"), 3) is not None, "History shows 7 after save")
    tap(wait(lambda n: n.cls.endswith("EditText")))
    type_text("booster")
    hide_keyboard()
    time.sleep(1)
    ids = [n.text.split(" ")[0] for n in dump() if re.match(r"INS-\d+ · ", n.text)]
    record("C07-ui", ids == ["INS-24091"], "Search booster -> " + ",".join(ids))
    tap(wait(lambda n: n.cls.endswith("EditText")))
    clear_field()
    hide_keyboard()
    tap(wait(desc_starts("Filter: Critical")))
    time.sleep(1)
    ids = [n.text.split(" ")[0] for n in dump() if re.match(r"INS-\d+ · ", n.text)]
    record("C08-ui", ids == ["INS-24072"], "Critical filter -> " + ",".join(ids))
    tap(wait(desc_starts("Filter: All")))
    bottom_tab("Dashboard")
    record("B07-dashboard", wait(lambda n: re.match(r"Good (morning|afternoon|evening)$", n.text), 10) is not None, "Bottom navigation reached Dashboard")


def s_cancel():
    tap(find_scroll(desc_starts("Air Handler 203,")))
    wait(text_is("Asset detail"), 15)
    tap(find_scroll(text_is("Start inspection")))
    wait(text_is("New inspection"), 15)
    tap(wait(desc_starts("Cancel inspection")))
    ok = wait(text_is("Asset detail"), 10)
    sh("input keyevent 4")
    bottom_tab("History")
    record("B11", ok is not None and wait(text_is("7 completed inspections"), 10) is not None, "Header back/cancel returned without saving (still 7)")


def s_restart():
    sh(f"am force-stop {PKG}")
    time.sleep(2)
    ms, _ = launch()
    wait(lambda n: re.match(r"Good (morning|afternoon|evening)$", n.text), 60)
    bottom_tab("History")
    found = wait(lambda n: n.text.startswith("INS-24092 · "), 15)
    shot("c10-history-after-restart")
    record("C10", found is not None, f"INS-24092 present after force-stop + relaunch (TotalTime={ms} ms)")
    record("J05", found is not None and wait(text_is("7 completed inspections"), 3) is not None, "Relaunch succeeded, data intact")


def s_fontscale():
    sh("settings put system font_scale 1.3")
    time.sleep(1)
    sh(f"am force-stop {PKG}")
    launch()
    wait(lambda n: re.match(r"Good (morning|afternoon|evening)$", n.text), 60)
    time.sleep(1.5)
    shot("f05-dashboard-font-1.3")
    reach = find_scroll(text_is("View all assets")) is not None
    bottom_tab("Assets")
    time.sleep(1)
    tap_scroll(desc_starts("Cooling Tower 07,"), "row")
    wait(text_is("Asset detail"), 15)
    tap(find_scroll(text_is("Start inspection")))
    wait(text_is("New inspection"), 15)
    time.sleep(1)
    shot("f05-form-font-1.3")
    sub = find_scroll(text_is("Submit inspection"))
    shot("f05-form-font-1.3-bottom")
    record("F05", reach and sub is not None, "At font scale 1.3 dashboard and form remain reachable")
    sh("settings put system font_scale 1.0")
    sh(f"am force-stop {PKG}")


for name, fn in [("launch", s_launch), ("assets", s_assets), ("detail", s_detail), ("back", s_back), ("form", s_form),
                 ("picker", s_picker), ("submit", s_submit), ("history", s_history), ("cancel", s_cancel),
                 ("restart", s_restart), ("fontscale", s_fontscale)]:
    step(name, fn)

crash = sh("logcat -d -b crash")
open(os.path.join(OUT, "logcat-crash.txt"), "w").write(crash)
record("J03", "FATAL EXCEPTION" not in crash and "com.fieldcheck.app" not in crash, "No crash-buffer entries for the app" if not crash.strip() else crash[:500])
with open(os.path.join(OUT, "logcat-app.txt"), "w") as f:
    f.write(adb("logcat", "-d", "-t", "3000"))
summary = {"total": len(checks), "pass": sum(c["status"] == "PASS" for c in checks), "fail": sum(c["status"] == "FAIL" for c in checks),
           "screen": f"{W}x{H} density {density}", "checks": checks}
json.dump(summary, open(os.path.join(OUT, "android-checks.json"), "w"), indent=2, ensure_ascii=False)
print(f"Checks: {summary['pass']} pass / {summary['fail']} fail of {summary['total']}")
