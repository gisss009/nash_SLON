import requests

url = "http://localhost:5000/photos/upload"
username = "ivan"
file_path = "C:/Users/diamantove/AppData/Local/Programs/Python/Python310/1.jpg"

with open(file_path, 'rb') as file:
    files = {'file': (file_path, file, 'image/jpeg')}
    data = {'username': username}

    response = requests.post(url, files=files, data=data)

print("Статус:", response.status_code)
print("Ответ:", response.text)


url = f"http://localhost:5000/photos/image/{username}"

response = requests.get(url)

if response.status_code == 200:
    content_type = response.headers.get('Content-Type', '')
    ext_map = {
        'image/jpeg': 'jpg',
        'image/png': 'png',
        'image/gif': 'gif'
    }
    ext = ext_map.get(content_type, 'jpg')

    filename = f"{username}_downloaded.{ext}"
    with open(filename, 'wb') as f:
        f.write(response.content)

    print(f"Изображение сохранено как {filename}")
else:
    print(f"Ошибка: статус {response.status_code} — {response.text}")
