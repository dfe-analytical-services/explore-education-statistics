import { useAuthContext } from '@admin/contexts/AuthContext';

export default function useIsMappingActionsReadOnly(readOnly: boolean) {
  const { user } = useAuthContext();

  return readOnly || !user?.permissions.canManagePublicApiDataSets;
}
