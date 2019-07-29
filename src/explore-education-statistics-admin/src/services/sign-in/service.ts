import PrototypeLoginService from "@admin/services/PrototypeLoginService";

export interface User {
  id: string;
  name: string;
  permissions: string[];
}

export interface Authentication {
  user?: User;
}

const setLoggedInUser = (email: string): Authentication => {
  PrototypeLoginService.setActiveUser('4add7621-4aef-4abc-b2e6-0938b37fe5b9');
  return PrototypeLoginService.getAuthentication('4add7621-4aef-4abc-b2e6-0938b37fe5b9');
};

export default {
  setLoggedInUser,
}