import React, { createContext, useContext, useState } from 'react';
import { Snackbar, Alert } from '@mui/material';

const NotifierContext = createContext();

export function NotifierProvider({ children }) {
  const [notif, setNotif] = useState({ open: false });

  const notify = (message, severity = 'info', duration = 3000) => {
    setNotif({ open: true, message, severity, duration });
  };

  const handleClose = () =>
    setNotif((prev) => ({ ...prev, open: false }));

  return (
    <NotifierContext.Provider value={{ notify }}>
      {children}
      <Snackbar
        open={notif.open}
        autoHideDuration={notif.duration}
        onClose={handleClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert severity={notif.severity} onClose={handleClose}>
          {notif.message}
        </Alert>
      </Snackbar>
    </NotifierContext.Provider>
  );
}

export const useNotifier = () => useContext(NotifierContext);