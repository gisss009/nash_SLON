from flask import Flask, current_app
from db import *
from users import bp1
from events import bp2
from likes import bp3
from photos import bp4
from url import bp5
import os

app = Flask(__name__)
app.register_blueprint(bp1)
app.register_blueprint(bp2)
app.register_blueprint(bp3)
app.register_blueprint(bp4) 
app.register_blueprint(bp5) 

# Абсолютный путь к папке uploads
basedir = os.path.abspath(os.path.dirname(__file__))
upload_folder = os.path.join(basedir, 'uploads')
os.makedirs(upload_folder, exist_ok=True)

# Сохраняем в конфиг
app.config['UPLOAD_FOLDER'] = upload_folder

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)