from aiogram import Router, types, F
from database.db import Database
from keyboards.inline import get_product_keyboard
from utils.helpers import format_product_card
router = Router()

@router.callback_query(F.data.startswith("product_"))
async def show_product(callback: types.CallbackQuery, db: Database):
    product_id = int(callback.data.split("_")[1])
    product = await db.get_product(product_id)
    if not product:
        await callback.answer("Товар не найден", show_alert=True)
        return
    text = format_product_card(product)
    if product.get('image_url'):
        await callback.message.answer_photo(
            photo=product['image_url'],
            caption=text,
            parse_mode="HTML",
            has_spoiler=True,
            reply_markup=get_product_keyboard())
    else:
        await callback.message.answer(
            text,
            parse_mode="HTML",
            reply_markup=get_product_keyboard())
    await callback.answer()
