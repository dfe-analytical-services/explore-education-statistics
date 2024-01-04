/* eslint-disable lines-between-class-members */
import dotenv from 'dotenv';

dotenv.config();
// eslint-disable-next-line import/prefer-default-export
export class environment {
  public static BASE_URL = process.env.PUBLIC_URL;
  public static ADMIN_URL = process.env.ADMIN_URL;
  public static ADMIN_EMAILADDR = process.env.ADMIN_EMAIL;
  public static ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;
}
