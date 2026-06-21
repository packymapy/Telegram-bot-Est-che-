import os
from dotenv import load_dotenv
load_dotenv()

class Config:
    BOT_TOKEN = os.getenv("BOT_TOKEN", "xxxx")
    DB_HOST = os.getenv("DB_HOST", "xxxx")
    DB_NAME = os.getenv("DB_NAME", "xxxx")
    DB_USER = os.getenv("DB_USER", "xxxx")
    DB_PASSWORD = os.getenv("DB_PASSWORD", "xxxx")
    MIN_AGE = 18
    MAX_VERIFICATION_ATTEMPTS = 3
    BLOCK_MINUTES = 60
    ADMIN_IDS = [xxxx]
config = Config()
