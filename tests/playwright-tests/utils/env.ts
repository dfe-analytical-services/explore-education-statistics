import dotenv from 'dotenv';

dotenv.config();
// eslint-disable-next-line import/prefer-default-export
export class environment {
  public static BASE_URL = process.env.PUBLICURL;
}
