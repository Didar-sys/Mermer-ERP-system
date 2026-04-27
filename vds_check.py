import socket
import urllib.request
import ssl
import sys

HOST = "awtofon.com.tm"

# Get IP
try:
    ip = socket.gethostbyname(HOST)
    print(f"IP: {ip}")
except Exception as e:
    print(f"DNS failed: {e}")
    sys.exit(1)

# Check HTTPS (port 443) - maybe there's a panel
ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

for url in [
    f"https://{HOST}/",
    f"https://{ip}/",
    f"https://{HOST}:8443/",
    f"https://{HOST}/phpmyadmin/",
    f"https://{HOST}:10000/",   # Webmin
    f"https://{HOST}:2083/",    # cPanel
    f"https://{HOST}:8888/",    # some panels
]:
    try:
        req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
        with urllib.request.urlopen(req, timeout=5, context=ctx) as resp:
            title_hint = resp.read(500).decode(errors="ignore")
            print(f"  OPEN: {url}  -> {title_hint[:80].strip()}")
    except urllib.error.HTTPError as e:
        print(f"  HTTP {e.code}: {url}")
    except Exception:
        print(f"  closed: {url}")
