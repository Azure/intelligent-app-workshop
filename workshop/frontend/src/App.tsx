import React, { useState, useEffect } from 'react';
import { ChakraProvider, Box, VStack, Input, Text, Container, useColorMode, useColorModeValue, IconButton, Flex, InputGroup, InputRightElement } from '@chakra-ui/react';
import { SunIcon, MoonIcon, ArrowForwardIcon } from '@chakra-ui/icons';
import axios from 'axios';
import { extendTheme, ThemeConfig } from '@chakra-ui/react';

interface Message {
  message: string;
  role: string;
}

// Azure color scheme
const colors = {
  azure: {
    50: '#e5f1fb',
    100: '#cce4f6',
    200: '#99c9ed',
    300: '#66ade3',
    400: '#3392da',
    500: '#0078d4', // Primary Azure color
    600: '#006abe',
    700: '#005ba7',
    800: '#004d8f',
    900: '#003e77',
  },
};

// Custom theme configuration
const config: ThemeConfig = {
  initialColorMode: "light",
  useSystemColorMode: false,
};

// Create a custom theme with Azure colors
const theme = extendTheme({ colors, config });

// Color mode toggle button component
const ColorModeToggle = () => {
  const { toggleColorMode } = useColorMode();
  const text = useColorModeValue("dark", "light");
  const SwitchIcon = useColorModeValue(MoonIcon, SunIcon);

  return (
    <IconButton
      aria-label={`Switch to ${text} mode`}
      variant="ghost"
      color="current"
      onClick={toggleColorMode}
      icon={<SwitchIcon />}
      size="md"
    />
  );
};

function App() {
  const [inputMessage, setInputMessage] = useState('');
  const [messageHistory, setMessageHistory] = useState<Message[]>([]);
  const [welcomeMessage, setWelcomeMessage] = useState<Message>({ message: 'You are a friendly financial advisor that only emits financial advice in a creative and funny tone', role: 'system' });
  const proxy_url = process.env.REACT_APP_PROXY_URL || '/api/chat';
  useEffect(() => {
    setMessageHistory([welcomeMessage]);
  }, []);

  const handleSendMessage = async () => {
    if (!inputMessage.trim()) return;
    if (!proxy_url) {
      console.error('Proxy URL is not set');
      return;
    }

    try {
      const result = await axios.post(proxy_url, {
        inputMessage,
        messageHistory
      }, {
        headers: {
          'Content-Type': 'application/json'
        }
      });

      if (result.data && result.data.messageHistory) {
        setMessageHistory(result.data.messageHistory);
      } else {
        const newUserMessage: Message = { message: inputMessage, role: 'user' };
        const newAssistantMessage: Message = {
          message: result.data.response || 'No response received',
          role: 'assistant'
        };
        setMessageHistory(prevHistory => [...prevHistory, newUserMessage, newAssistantMessage]);
      }
      setInputMessage('');
    } catch (error) {
      console.error('Error sending message:', error);
      const errorMessage: Message = { message: 'Error occurred while sending message', role: 'system' };
      setMessageHistory(prevHistory => [...prevHistory, errorMessage]);
    }
  };

  const bgColor = useColorModeValue("azure.50", "azure.900");
  const textColor = useColorModeValue("azure.900", "azure.50");

  return (
    <ChakraProvider theme={theme}>
      <Box minH="100vh" bg={bgColor} color={textColor}>
        <Container maxW="container.md" py={10}>
          <Flex justifyContent="space-between" alignItems="center" mb={4}>
            <Text fontSize="2xl" fontWeight="bold">Simple Copilot chat</Text>
            <ColorModeToggle />
          </Flex>
          <VStack spacing={4}>
            <Box w="100%" h="300px" overflowY="auto" borderWidth={1} borderRadius="md" p={4} bg={useColorModeValue("white", "gray.800")}>
              {messageHistory.map((msg, index) => (
                <Text key={index} fontWeight={msg.role === 'user' ? 'bold' : 'normal'}>
                  {msg.role}: {msg.message}
                </Text>
              ))}
            </Box>
            <InputGroup>
              <Input
                placeholder="Enter your message here and press Enter or click the arrow to send"
                value={inputMessage}
                onChange={(e) => setInputMessage(e.target.value)}
                onKeyPress={(e) => {
                  if (e.key === 'Enter') {
                    handleSendMessage();
                  }
                }}
                bg={useColorModeValue("white", "gray.800")}
              />
              <InputRightElement>
                <IconButton
                  aria-label="Send message"
                  icon={<ArrowForwardIcon />}
                  onClick={handleSendMessage}
                  colorScheme="azure"
                />
              </InputRightElement>
            </InputGroup>
          </VStack>
        </Container>
      </Box>
    </ChakraProvider>
  );
}

export default App;
