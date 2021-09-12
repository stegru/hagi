import React from "react";
import { Button, Modal } from "react-bootstrap";

class InitialSetup extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      username: "",
      password: ""
    };
  }

  render() {
    return (
      <Modal show={true}>
        <Modal.Header>
          <Modal.Title>Welcome to HAGI</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          The service has not been configured yet.
        </Modal.Body>
        <Modal.Footer>
          <Button>OK</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}


export default InitialSetup;
