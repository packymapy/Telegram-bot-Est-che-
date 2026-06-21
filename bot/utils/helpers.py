from datetime import datetime

def format_product_card(product: dict, short: bool = False) -> str:
    text = f"<b>{product['name']}</b>\n"
    if product.get('brand_name'):
        text += f"🏷️ {product['brand_name']}\n"
    text += f"💰 {product['price']} руб.\n"
    if product.get('category_name'):
        text += f"📂 {product['category_name']}\n"
    if not short and product.get('description'):
        desc = product['description'][:200]
        if len(product['description']) > 200:
            desc += "..."
        text += f"\n{desc}"
    return text
  
def get_age_from_date(birth_date: datetime) -> int:
    today = datetime.now().date()
    return today.year - birth_date.year - (
        (today.month, today.day) < (birth_date.month, birth_date.day))

def is_admin_user(tg_id: int, admin_ids: list) -> bool:
    return tg_id in admin_ids
