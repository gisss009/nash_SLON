from flask import Flask
from db import *
from users import bp1
from events import bp2
from likes import bp3

app = Flask(__name__)
app.register_blueprint(bp1)
app.register_blueprint(bp2)
app.register_blueprint(bp3)

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)