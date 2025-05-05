from flask import Blueprint
from db import *
from flask import request, jsonify, send_from_directory, current_app
from werkzeug.utils import secure_filename
import os
from PIL import Image

bp4 = Blueprint('photos', __name__,)

# UPLOAD_FOLDER = 'uploads'
ALLOWED_EXTENSIONS = {'png', 'jpg', 'jpeg', 'gif'}
# os.makedirs(UPLOAD_FOLDER, exist_ok=True)

# Функция: проверка расширения
def allowed_file(filename):
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS

@bp4.route('/photos/upload', methods=['POST'])
def upload_image():
    if 'file' not in request.files or 'username' not in request.form:
        return jsonify({'ok': False, 'response': 'file и username обязательны'}), 400

    file = request.files['file']
    username = request.form['username'].strip()

    if file.filename == '':
        return jsonify({'ok': False, 'response': 'Имя файла пустое'}), 400

    # Получение расширения
    ext = os.path.splitext(file.filename)[1].lower()
    if ext not in ['.jpg', '.jpeg', '.png']:
        return jsonify({'ok': False, 'response': 'Поддерживаются только JPG и PNG'}), 400

    filename = f"{username}{ext}"
    upload_folder = current_app.config['UPLOAD_FOLDER']
    os.makedirs(upload_folder, exist_ok=True)
    save_path = os.path.join(upload_folder, filename)

    # Сжатие изображения
    try:
        img = Image.open(file.stream)

        if ext in ['.jpg', '.jpeg']:
            img = img.convert("RGB")
            img.save(save_path, format='JPEG', quality=70, optimize=True)
        elif ext == '.png':
            img.save(save_path, format='PNG', optimize=True)

        return jsonify({'ok': True, 'response': f'Файл {filename} успешно загружен'}), 200

    except Exception as e:
        return jsonify({'ok': False, 'response': f'Ошибка обработки изображения: {str(e)}'}), 500

@bp4.route('/photos/image/<username>', methods=['GET'])
def get_user_image(username):
    username = secure_filename(username)

    for ext in ALLOWED_EXTENSIONS:
        filename = f"{username}.{ext}"
        file_path = os.path.join(current_app.config['UPLOAD_FOLDER'], filename)
        if os.path.exists(file_path):
            return send_from_directory(current_app.config['UPLOAD_FOLDER'], filename)

    # Если не найдено
    return jsonify({'ok': False}), 400
