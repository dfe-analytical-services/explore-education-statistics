import React from 'react';

import { FormSelect, FormGroup } from '@common/components/form';
import PrototypeLoginService from '@admin/services/PrototypeLoginService';
import Link from '@admin/components/Link';
import PrototypePage from './components/PrototypePage';

function PrototypesIndexPage() {
  return (
    <PrototypePage>
      <h1>Index page for administrative application</h1>

      <h3>Prototypes</h3>
      <FormGroup>
        <FormSelect
          id="changeUser"
          name="changeUser"
          label="Change Prototype User"
          options={PrototypeLoginService.getUserList().map(user => ({
            value: user.id,
            label: user.name,
          }))}
          onChange={e => {
            PrototypeLoginService.setActiveUser(e.target.value);
          }}
        />
      </FormGroup>
      <ul>
        <li>
          <Link to="/prototypes/admin-dashboard">
            Administrators' dashboard page
          </Link>
        </li>
      </ul>

      <h3>Tools</h3>
      <ul>
        <li>
          <a href="/tools">Administrative tools</a>
        </li>
        <li>
          <a href="/tools/release/notify">Send a release notification</a>
        </li>
      </ul>
    </PrototypePage>
  );
}

export default PrototypesIndexPage;
