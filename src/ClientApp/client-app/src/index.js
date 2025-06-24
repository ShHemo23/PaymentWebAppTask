import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';

// Define a modern theme
const modernTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#0D47A1',        // your deep-blue brand color
      light: '#5472d3',
      dark: '#002171',
      contrastText: '#fff',
    },
    secondary: {
      main: '#FFC107',        // your gold accent
      contrastText: '#000',
    },
    error: {
      main: '#D32F2F',
    },
    background: {
      default: '#F5F5F5',
      paper:   '#FFFFFF',
    },
  },
  typography: {
    fontFamily: `'Roboto', 'Helvetica', 'Arial', sans-serif`,
    h1: { fontSize: '2.5rem', fontWeight: 700 },
    h2: { fontSize: '2rem',   fontWeight: 600 },
    body1: { fontSize: '1rem', fontWeight: 400 },
    button: { textTransform: 'none', fontWeight: 500 },
  },
   shape: {
    borderRadius: 8, // Keep slightly rounded corners as before
  },
  components: {
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: '#0D47A1', // Use primary dark for AppBar background
          boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          width: 240,
          backgroundColor: '#FFFFFF', // White background for drawer
          borderRight: '1px solid #EEE', // Subtle border
        },
      },
    },
    MuiListItemButton: {
        styleOverrides: {
            root: {
                borderRadius: 8, // Apply border radius to list items
                margin: '4px 8px', // Add some margin
                paddingLeft: '16px', // Adjust padding
                '&.Mui-selected': { 
                    backgroundColor: '#FFC107', // Secondary color for selected
                    color: '#000', // Black text for contrast
                },
                 transition: 'transform 0.2s, box-shadow 0.2s', // Add transition
                 '&:hover': {
                     transform: 'translateY(-2px)', // Slight lift effect
                     boxShadow: '0 4px 8px rgba(0,0,0,0.1)', // Add shadow on hover
                 },
            }
        }
    }
  }
});

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <ThemeProvider theme={modernTheme}>
      <CssBaseline /> {/* Add CssBaseline for consistent styling */}
      <App />
    </ThemeProvider>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
